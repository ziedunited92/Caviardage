using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions;
using Tesseract;
using iTextSharp.text;
using iTextSharp.text.pdf;

namespace Caviardage.Controllers
{
    public class CaviardageController : Controller
    {
        public ActionResult UploadPassport()
        {
            return View();
        }

        [HttpPost]
        public IActionResult ProcessPassport(IFormFile passportImage)
        {
            string outputPath = @"C:\Users\User\Documents\ziedCaviarde.pdf"; // Remplacez par le chemin de sortie souhaité
            if (passportImage != null && passportImage.Length > 0)
            {
                try
                {
                    using (var engine = new TesseractEngine(@"./tessdata", "eng", EngineMode.Default))
                    {
                        using (var memoryStream = new MemoryStream())
                        {
                            passportImage.CopyTo(memoryStream);
                            memoryStream.Position = 0; // Reset the position to the beginning of the stream

                            var doc = new Aspose.Words.Document(memoryStream);



                           doc.Save($"Output.tiff");


                            using (var img = Pix.LoadFromFile($"Output.tiff"))
                            {
                                using (var page = engine.Process(img))
                                {
                                    // Récupération du texte complet
                                    string texteComplet = page.GetText();
                                    texteComplet = texteComplet.Replace("Evaluation Only. Created with Aspose.Words. Copyright 2003-2024 Aspose Pty Ltd.", "");
                                    texteComplet = texteComplet.Replace("Created with an evaluation copy of Aspose.Words. To discover the full versions of our APIs\r\nplease ttps://products.aspose.com/words/", "");

                                    // Remplacer les informations sensibles par des étoiles
                                    string texteAffiche = RedigerPII(texteComplet);

                                    ViewBag.MyString = texteAffiche;

                                    iTextSharp.text.Document documentPdf = new iTextSharp.text.Document(PageSize.A4);

                                    // Création du fichier de sortie
                                    PdfWriter.GetInstance(documentPdf, new FileStream(outputPath, FileMode.Create));

                                    // Ouverture du document
                                    documentPdf.Open();


                                    // Ajout du texte au document
                                    documentPdf.Add(new iTextSharp.text.Paragraph(texteAffiche));
                                    // Fermeture du document
                                    documentPdf.Close();



                                  
                                }
                            }
                        }
                    }
                }
                catch
                {
                    ViewBag.Error = "An error occurred during processing. Please try again.";
                }
            }
            else
            {
                ViewBag.Error = "Please upload an image.";
            }
           

            return View("AffichageResultat");
        }
        static string RedigerPII(string texte)
        {
            // Remplacer les informations sensibles par des étoiles
            string texteRedige = Regex.Replace(texte, @"\b(?:\d{3}-\d{3}-\d{3}|\d{4}-\d{4}-\d{4}-\d{4}|[A-Z][a-z]+)\b", "***");

            return texteRedige;
        }

    }
}
