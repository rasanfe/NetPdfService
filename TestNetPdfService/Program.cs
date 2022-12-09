using NetPdfService;
using System;
using System.Reflection;
using System.Runtime.Intrinsics.X86;

namespace AppPdfService
{
    internal class Program
    {
        static void Main(string[] args)
        {
            string errorText = "";

            if (args.Length != 0 && args.Length != 7 && args.Length != 14)
            {
                errorText = "(" + args.Length + ") Numero de Arrgumentos de entrada Incorrecto";
                Console.WriteLine(errorText);
                return;
            }

            string inFile = "";
            string outFile = "";
            string cerFile = "";
            string password = "";
            string reason = "";
            string location = "";
            string contact = "";
            string imageFile = "";
            int x1 = 0;
            int y1 = 0;
            int x2 = 0;
            int y2 = 0;
            string nombre = "";
            string dni = "";

            if (args.Length == 0)
            {
                //Sin Argumentos de entrada usao la app para hacer test de la libreria:
                Console.WriteLine("Probamos el metodo: Firmar (Visual)");

                PdfService lnpdf = new PdfService();

                string? path = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
                string lastPath = path.Split(Path.DirectorySeparatorChar).Last();
                if (lastPath == "publish")
                {
                    path = "..\\..\\..\\..";
                }
                else
                {
                    path = "..\\..\\..";
                }

                inFile = path + "\\TestSource\\prueba_visual.pdf";
                outFile = path + "\\TestResult\\prueba_visual_sign.pdf";
                cerFile = path + "\\TestSource\\firma.pfx";
                password = "PDFSIGN";
                reason = "proof of authenticity";
                location = "";
                contact = "";
                imageFile = path + "\\TestSource\\firma.jpg";
                x1 = 228;
                y1 = 45;
                x2 = 150;//378;
                y2 = 80;//125;
                nombre = "Prueba Yo Tu";
                dni = "1234578z";

                if (File.Exists(outFile))
                {
                    File.Delete(outFile);
                }

                try
                {
                    lnpdf.Firmar(inFile, outFile, cerFile, password, reason, location, contact, imageFile, x1, y1, x2, y2, nombre, dni);
                    Console.WriteLine("resultado ok. archivo prueba_visual_sign.pdf creado con éxito.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine("error ejecutanto test firmar (visual). " + ex.Message);
                }

                Console.WriteLine("");
                Console.WriteLine("");

                Console.WriteLine("Probamos el metodo: Firmar (NO Visual)");


                inFile = path + "\\TestSource\\prueba.pdf";
                outFile = path + "\\TestResult\\prueba_sign.pdf";
                cerFile = path + "\\TestSource\\firma.pfx";
                password = "PDFSIGN";
                reason = "proof of authenticity";
                location = "";
                contact = "";

                if (File.Exists(outFile))
                {
                    File.Delete(outFile);
                }

                try
                {
                    lnpdf.Firmar(inFile, outFile, cerFile, password, reason, location, contact);
                    Console.WriteLine("Resultado OK. Archivo prueba_sign.pdf Creado con éxito.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error ejecutanto test Firmar (No Visual). " + ex.Message);
                }

                System.Diagnostics.Process.Start("explorer.exe", path + "\\TestResult");
                return;

            }
            else
            {

                inFile = args[0];
                outFile = args[1];
                cerFile = args[2];
                password = args[3];
                reason = args[4];
                location = args[5];
                contact = args[6];
                imageFile = "";
                x1 = 0;
                y1 = 0;
                x2 = 0;
                y2 = 0;
                nombre = "";
                dni = "";


                if (args.Length > 7)
                {
                    imageFile = args[7];
                    x1 = Int32.Parse(args[8]);
                    y1 = Int32.Parse(args[9]);
                    x2 = Int32.Parse(args[10]);
                    y2 = Int32.Parse(args[11]);
                    nombre = args[12];
                    dni = args[13];
                }


                if (File.Exists(outFile))
                {
                    File.Delete(outFile);
                }

                try
                {
                    PdfService lnpdf = new PdfService();
                    if (args.Length > 7)
                    {
                        lnpdf.Firmar(inFile, outFile, cerFile, password, reason, location, contact, imageFile, x1, y1, x2, y2, nombre, dni);
                    }
                    else
                    {
                        lnpdf.Firmar(inFile, outFile, cerFile, password, reason, location, contact);
                    }
                    Console.WriteLine("Done!");

                }
                catch (Exception ex)
                {
                    errorText = ex.Message;
                    Console.WriteLine("Error: " + errorText);
                }
                return;
            }
        }
    }
}