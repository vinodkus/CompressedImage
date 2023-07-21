using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Drawing.Imaging;

namespace CompressImage
{
    public partial class WebForm1 : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        protected void btnCompress_Click(object sender, EventArgs e)
        {
            if (fileUpload.HasFile)
            {
                try
                {
                    // Get the file name and extension
                    string fileName = Path.GetFileNameWithoutExtension(fileUpload.FileName);
                    string extension = Path.GetExtension(fileUpload.FileName);

                    // Create a byte array to store the compressed image data
                    byte[] compressedImageData;
                    double compressedImageSizeKB;


                    using (System.Drawing.Image originalImage = System.Drawing.Image.FromStream(fileUpload.PostedFile.InputStream))
                    {
                        // Compress the image to a JPEG format with 80% quality
                        using (MemoryStream memoryStream = new MemoryStream())
                        {
                            System.Drawing.Imaging.ImageCodecInfo jpegCodec = GetEncoderInfo("image/jpeg");
                            System.Drawing.Imaging.Encoder qualityEncoder = System.Drawing.Imaging.Encoder.Quality;
                            EncoderParameters encoderParameters = new EncoderParameters(1);
                            EncoderParameter qualityParameter = new EncoderParameter(qualityEncoder, 80L);
                            encoderParameters.Param[0] = qualityParameter;

                            originalImage.Save(memoryStream, jpegCodec, encoderParameters);
                            compressedImageData = memoryStream.ToArray();
                            compressedImageSizeKB = compressedImageData.Length / 1024.0;
                        }
                    }

                    // Save the compressed image data into the database
                    string connectionString = "Server=LAPTOP-HJ6GEJME\\SQLEXPRESS;Database=vinod;Trusted_Connection=True;MultipleActiveResultSets=true";
                    using (SqlConnection connection = new SqlConnection(connectionString))
                    {
                        connection.Open();
                        string insertQuery = "INSERT INTO CompressedImages (ImageData,CompressedSizeKB) VALUES (@ImageData, @CompressedSizeKB)";
                        using (SqlCommand command = new SqlCommand(insertQuery, connection))
                        {
                            command.Parameters.AddWithValue("@ImageData", compressedImageData);
                            command.Parameters.AddWithValue("@CompressedSizeKB", compressedImageSizeKB);
                            command.ExecuteNonQuery();
                        }
                    }

                    resultLabel.Text = "Image compressed and saved successfully!";
                }
                catch (Exception ex)
                {
                    resultLabel.Text = $"Error: {ex.Message}";
                }
            }
            else
            {
                resultLabel.Text = "Please select an image to compress and save.";
            }
        }
        private static System.Drawing.Imaging.ImageCodecInfo GetEncoderInfo(string mimeType)
        {
            System.Drawing.Imaging.ImageCodecInfo[] codecs = System.Drawing.Imaging.ImageCodecInfo.GetImageEncoders();
            foreach (System.Drawing.Imaging.ImageCodecInfo codec in codecs)
            {
                if (codec.MimeType == mimeType)
                {
                    return codec;
                }
            }
            return null;
        }
    }
}