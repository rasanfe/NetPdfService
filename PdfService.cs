﻿using System;
using System.Text;
using iText.Kernel.Pdf;
using iText.Signatures;
using iText.Kernel.Geom;
using Org.BouncyCastle.X509;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Pkcs;
using System.Globalization;
using iText.Kernel.Font;
using iText.IO.Image;
using System.IO;
using iText.Bouncycastle.X509;
using iText.Commons.Bouncycastle.Cert;
using iText.Bouncycastle.Crypto;
using iText.Commons.Bouncycastle.Cert;

namespace NetPdfService
{
    public class PdfService
    {

        private string errorText = "";
        public void Firmar(string inFile, string outFile, string certFile, string password, string reason, string location, string contact, string imgeFile, int x1, int y1, int x2, int y2, string nombre, string dni)
        {

            const bool isVisible = true;

            if (String.IsNullOrEmpty(imgeFile))
            {
                errorText = "Image File cannot be null";
                throw new ArgumentNullException(paramName: nameof(imgeFile), message: errorText);
            }

            if (String.IsNullOrWhiteSpace(nombre)) { nombre = ""; }
            if (String.IsNullOrWhiteSpace(nombre)) { dni = ""; }

            Firmar(inFile, outFile, certFile, password, reason, location, contact, imgeFile, x1, y1, x2, y2, nombre, dni, isVisible);
        }
        public void Firmar(string inFile, string outFile, string certFile, string password, string reason, string location, string contact)
        {
            const string imgeFile = "";
            const int x1 = 0;
            const int y1 = 0;
            const int x2 = 0;
            const int y2 = 0;
            const string nombre = "";
            const string dni = "";
            const bool isVisible = false;

            Firmar(inFile, outFile, certFile, password, reason, location, contact, imgeFile, x1, y1, x2, y2, nombre, dni, isVisible);

        }

        internal void Firmar(string inFile, string outFile, string certFile, string password, string reason, string location, string contact, string imgeFile, int x1, int y1, int x2, int y2, string nombre, string dni, bool isVisible)
        {
            if (String.IsNullOrEmpty(inFile))
            {
                errorText = "Input File cannot be null";
                throw new ArgumentNullException(paramName: nameof(inFile), message: errorText);
            }
            if (System.IO.Path.GetExtension(inFile) != ".pdf")
            {
                errorText = "Input File Extension is not PDF";
                throw new ArgumentException(paramName: nameof(inFile), message: errorText);
            }

            if (String.IsNullOrEmpty(outFile))
            {
                errorText = "Output File cannot be null";
                throw new ArgumentNullException(paramName: nameof(outFile), message: errorText);
            }
            if (System.IO.Path.GetExtension(outFile) != ".pdf")
            {
                errorText = "Output File Extension is not PDF";
                throw new ArgumentException(paramName: nameof(outFile), message: errorText);
            }
            if (String.IsNullOrEmpty(certFile))
            {
                errorText = "Certificate File File cannot be null";
                throw new ArgumentNullException(paramName: nameof(certFile), message: errorText);
            }
            if (System.IO.Path.GetExtension(certFile) != ".pfx")
            {
                errorText = "Certificate File Extension is not PFX";
                throw new ArgumentException(paramName: nameof(certFile), message: errorText);
            }
            if (String.IsNullOrEmpty(password))
            {
                errorText = "Password cannot be null";
                throw new ArgumentNullException(paramName: nameof(password), message: errorText);
            }

            if (String.IsNullOrWhiteSpace(reason)) { reason = "proof of authenticity"; }

            ResetError();

            try
            {
                Sign(inFile, outFile, certFile, password, reason, location, contact, imgeFile, x1, y1, x2, y2, nombre, dni, isVisible, null, null, null, 0);
            }
            catch (Exception ex)
            {
                errorText = ex.Message;
            }

        }


        internal void Sign(string inFile, string outFile, string certFile, string password, string reason, string location, string contact, string imgeFile, int x1, int y1, int x2, int y2, string nombre, string dni, bool isVisible,
            ICollection<ICrlClient>? crlList, IOcspClient? ocspClient, ITSAClient? tsaClient, int estimatedSize)
        {
            PdfSigner signer = new PdfSigner(new PdfReader(inFile), new FileStream(outFile, FileMode.Create), new StampingProperties());
            signer.SetCertificationLevel(PdfSigner.NOT_CERTIFIED);

            //Determinamos la Fecha de la certFile
            DateTime fechaFirma = DateTime.Now;
            signer.SetSignDate(fechaFirma);
            signer.SetFieldName("sig_" + dni + "_" + fechaFirma.ToString("U", DateTimeFormatInfo.InvariantInfo).Replace(" ", "_").Replace(",", "").Replace(":", ""));
            signer.SetCertificationLevel(PdfSigner.CERTIFIED_FORM_FILLING);
            signer.SetLocation(@location);
            signer.SetReason(@reason);
            signer.SetContact(@contact);

            // Create the signature appearance
            PdfSignatureAppearance appearance = signer.GetSignatureAppearance();


            if (isVisible)
            {
                //La Firma será Visible y en la Ultima página del PDF
                Rectangle rect = new Rectangle(x1, y1, x2, y2);
                int numberOfPages = GetNumberOfPages(inFile);
                ImageData imageData = ImageDataFactory.Create(imgeFile);
                appearance.SetImage(imageData);
                appearance.SetReuseAppearance(false);
                appearance.SetPageRect(rect);
                appearance.SetPageNumber(numberOfPages);
                appearance.SetImageScale(0.22f);

                //Añadimos el Nombre y el DNI en la firma como Texto.
                StringBuilder buf = new StringBuilder();
                buf.Append('\n').Append('\n').Append('\n').Append('\n').Append(@nombre).Append('\n').Append(@dni);
                string text = buf.ToString();

                //PdfFont font = PdfFontFactory.CreateFont(iText.IO.Font.Constants.StandardFonts.HELVETICA_BOLD);
                //appearance.SetLayer2Font(font);
                appearance.SetLayer2Text(text);
            }

            IX509Certificate[]? chain = null;
            IExternalSignature? pks = null;

            CreateChainFromFile(certFile, password, DigestAlgorithms.SHA256, ref chain, ref pks);

            // Aquí es donde necesitas convertir el arreglo de certificados al tipo esperado por iText
            var convertedChain = chain.Select(x => (iText.Commons.Bouncycastle.Cert.IX509Certificate)x).ToArray();

            signer.SignDetached(pks, convertedChain, crlList, ocspClient, tsaClient, estimatedSize, PdfSigner.CryptoStandard.CMS);
        }


        internal void CreateChainFromFile(String certFile, String password, String digestAlgorithm, ref IX509Certificate[]? chain, ref IExternalSignature? pks)
        {

            FileStream certStream = new FileStream(certFile, FileMode.Open, FileAccess.Read);
            try
            {
                Pkcs12Store pk12 = new Pkcs12StoreBuilder().Build();
                pk12.Load(certStream, password.ToCharArray());

                String alias = "";
                foreach (String tAlias in pk12.Aliases)
                {
                    if (pk12.IsKeyEntry(tAlias))
                    {
                        alias = tAlias;
                        break;
                    }
                }

                ICipherParameters pk = pk12.GetKey(alias).Key;
                X509CertificateEntry[] ce = pk12.GetCertificateChain(alias);
                chain = new IX509Certificate[ce.Length];
                for (int k = 0; k < ce.Length; ++k)
                {
                    chain[k] = new X509CertificateBC(ce[k].Certificate);
                }


                pks = new PrivateKeySignature(new PrivateKeyBC(pk), digestAlgorithm);


            }
            finally
            {
                certStream.Close();
            }

        }

        internal int GetNumberOfPages(string inputFile)
        {
            PdfReader reader = new PdfReader(inputFile);

            PdfDocument srcDoc = new PdfDocument(reader);
            int numberOfPages = srcDoc.GetNumberOfPages();
            srcDoc.Close();
            reader.Close();

            return numberOfPages;
        }
        public string GetLastError()
        {
            return errorText;
        }
        internal void ResetError()
        {
            errorText = "";
        }

    }
}