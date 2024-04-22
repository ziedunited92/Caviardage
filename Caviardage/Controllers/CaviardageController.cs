using Aspose.Words;
using Aspose.Words.Saving;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.VisualBasic;
using System.Reflection.Metadata;
using System.Web.WebPages;
using Tesseract;

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
            string outputPath = @"C:\Users\User\Documents\ziedCaviarde.doc"; // Remplacez par le chemin de sortie souhaité
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

                                    // Trouver l'index du champ "Numéro de carte de crédit"
                                    int indexDebutCarteCredit = texteComplet.IndexOf("Credit card Number", StringComparison.OrdinalIgnoreCase);

                                    // Caviarder les 16 chiffres après le champ
                                    const int longueurCarteCredit = 16;
                                    string texteCaviarde = texteComplet.Substring(indexDebutCarteCredit + 24, longueurCarteCredit);
                                    texteCaviarde = texteCaviarde.Replace("Cr", "");
                                    // Remplacer les chiffres par des étoiles
                                    string texteCaviardeRedacte = new string('*', longueurCarteCredit);
                                  string  texteAffiche = texteComplet.Replace(texteCaviarde, texteCaviardeRedacte);
                                    ViewBag.MyString = texteAffiche;
                                    // Créez un document Word vide
                                    Aspose.Words.Document documentWord = new Aspose.Words.Document();
                         documentWord.Cleanup();

                                   string content= documentWord.GetText();
                                    documentWord= (Aspose.Words.Document)content.Concat(texteAffiche);

                                    // Enregistrez le document Word généré
                                    documentWord.Save(@"C:\Users\User\Documents\ziedCaviarde.doc");
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

    }
}
