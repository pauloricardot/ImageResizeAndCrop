
public static class Utils{
public class GetThumbLocal
{
    
    public static string GetThumb(string img)
    {
        string urlImage = string.Empty;

        var filename = System.IO.Path.GetFileName(img);
        var imgThumb = System.Web.HttpContext.Current.Server.MapPath("~/Media/Thumbs/" + filename);


        if (System.IO.File.Exists(imgThumb))
        {
            urlImage = "/Media/Thumbs/" + filename;

        }
        else
        {

            urlImage = img;
          
            
        }

        return urlImage;

    }

}

#region Imager Resize

public static class Imager
{
    public static bool GenerateThumb()
    {
        var diretorio = System.Web.HttpContext.Current.Server.MapPath("~/Media/");
       
        var files = Directory.EnumerateFiles(diretorio, "*.*", SearchOption.TopDirectoryOnly)
        .Where(s => s.EndsWith(".png") || s.EndsWith(".jpg") || s.EndsWith(".JPG") || s.EndsWith(".PNG"));



        foreach (var item in files)
        {

            var img = item.Substring(item.LastIndexOf("\\") + 1);

            var imgExists = item.Replace("Media", "Media\\Thumbs");

            if (!System.IO.File.Exists(imgExists))
            {
                CropAndResizeUpload(diretorio,img,400,400);
            }
           

        }


        return true;
    }

   
    
    public static void CropAndResizeUpload(string pathThumb, string filename, int width, int height)
    {
        

        try
        {
            System.Drawing.Image imgBef;
            imgBef = System.Drawing.Image.FromFile(pathThumb + filename);


            System.Drawing.Image _imgR;
            _imgR = Imager.Resize(imgBef, width, height, true);


            System.Drawing.Image _img2;
            _img2 = Imager.PutOnWhiteCanvas(_imgR, width, height);

            //Save JPEG  
            string path = System.Web.HttpContext.Current.Server.MapPath("~/Media/Thumbs/");
            System.Drawing.Image _imgCrop;
            _imgCrop = Imager.Crop(_img2, new Rectangle(0, 0, 399, 300));
            Imager.SaveJpeg(path + filename, _imgCrop);


        }
        catch (System.IO.FileNotFoundException e)
        {
           
        }
    }

   

    /// <summary>  
    /// Save image as jpeg  
    /// </summary>  
    /// <param name="path">path where to save</param>  
    /// <param name="img">image to save</param>  
    public static void SaveJpeg(string path, System.Drawing.Image img)
    {
        var qualityParam = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, 100L);
        var jpegCodec = GetEncoderInfo("image/jpeg");

        var encoderParams = new EncoderParameters(1);
        encoderParams.Param[0] = qualityParam;

        
        //img.Save(path, jpegCodec, encoderParams);
        //img.Dispose();

        using (System.IO.MemoryStream lMemoryStream = new System.IO.MemoryStream())
        {
            //aImage.Save(lMemoryStream, aImageFormat);
            img.Save(lMemoryStream, jpegCodec, encoderParams);

            using (System.IO.FileStream lFileStream = new System.IO.FileStream(path, System.IO.FileMode.Create))
            {
                lMemoryStream.Position = 0;

                lMemoryStream.CopyTo(lFileStream);
            }
        }

    }

    /// <summary>  
    /// Save image  
    /// </summary>  
    /// <param name="path">path where to save</param>  
    /// <param name="img">image to save</param>  
    /// <param name="imageCodecInfo">codec info</param>  
    public static void Save(string path, System.Drawing.Image img, ImageCodecInfo imageCodecInfo)
    {
        var qualityParam = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, 100L);

        var encoderParams = new EncoderParameters(1);
        encoderParams.Param[0] = qualityParam;
        img.Save(path, imageCodecInfo, encoderParams);
    }

    /// <summary>  
    /// get codec info by mime type  
    /// </summary>  
    /// <param name="mimeType"></param>  
    /// <returns></returns>  
    public static ImageCodecInfo GetEncoderInfo(string mimeType)
    {
        return ImageCodecInfo.GetImageEncoders().FirstOrDefault(t => t.MimeType == mimeType);
    }

    /// <summary>  
    /// the image remains the same size, and it is placed in the middle of the new canvas  
    /// </summary>  
    /// <param name="image">image to put on canvas</param>  
    /// <param name="width">canvas width</param>  
    /// <param name="height">canvas height</param>  
    /// <param name="canvasColor">canvas color</param>  
    /// <returns></returns>  
    public static System.Drawing.Image PutOnCanvas(System.Drawing.Image image, int width, int height, Color canvasColor)
    {

        var res = new Bitmap(width, height);
        using (var g = Graphics.FromImage(res))
        {
            g.Clear(canvasColor);
            var x = 0;  //(width - image.Width) / 2;
            var y = 0; //(height - image.Height) / 2;
                       // g.DrawImage(image, new Rectangle(0,0, 399, 300),new Rectangle(0, 0, image.Width, image.Height),GraphicsUnit.Pixel);
            g.DrawImageUnscaledAndClipped(image, new Rectangle(x, y, 399, 300));
            //g.DrawImage(image, 0, 0, image.Width, image.Height);
            //g.DrawImageUnscaled(image, x, y, image.Width, image.Height);
        }

        return res;
    }

    /// <summary>  
    /// the image remains the same size, and it is placed in the middle of the new canvas  
    /// </summary>  
    /// <param name="image">image to put on canvas</param>  
    /// <param name="width">canvas width</param>  
    /// <param name="height">canvas height</param>  
    /// <returns></returns>  
    public static System.Drawing.Image PutOnWhiteCanvas(System.Drawing.Image image, int width, int height)
    {

        return PutOnCanvas(image, width, height, Color.White);
    }

    /// <summary>  
    /// resize an image and maintain aspect ratio  
    /// </summary>  
    /// <param name="image">image to resize</param>  
    /// <param name="newWidth">desired width</param>  
    /// <param name="maxHeight">max height</param>  
    /// <param name="onlyResizeIfWider">if image width is smaller than newWidth use image width</param>  
    /// <returns>resized image</returns>  
    public static System.Drawing.Image Resize(System.Drawing.Image image, int newWidth, int maxHeight, bool onlyResizeIfWider)
    {

        if (onlyResizeIfWider && image.Width <= newWidth) newWidth = image.Width;

        var newHeight = image.Height * newWidth / image.Width;
        if (newHeight > maxHeight)
        {
            // Resize with height instead  
            newWidth = image.Width * maxHeight / image.Height;
            newHeight = maxHeight;
        }

        var res = new Bitmap(newWidth, newHeight);

        using (var graphic = Graphics.FromImage(res))
        {
            graphic.InterpolationMode = InterpolationMode.HighQualityBicubic;
            graphic.SmoothingMode = SmoothingMode.HighQuality;
            graphic.PixelOffsetMode = PixelOffsetMode.HighQuality;
            graphic.CompositingQuality = CompositingQuality.HighQuality;
            graphic.DrawImage(image, 0, 0, newWidth, newHeight);
        }


        return res;
    }

    /// <summary>  
    /// Crop an image   
    /// </summary>  
    /// <param name="img">image to crop</param>  
    /// <param name="cropArea">rectangle to crop</param>  
    /// <returns>resulting image</returns>  
    public static System.Drawing.Image Crop(System.Drawing.Image img, Rectangle cropArea)
    {
        var bmpImage = new Bitmap(img);
        var bmpCrop = bmpImage.Clone(cropArea, bmpImage.PixelFormat);
        return bmpCrop;
    }

    public static System.Drawing.Image Crop(System.Drawing.Image img)
    {
        Rectangle cropArea = new Rectangle(0, 0, 500, 500);
        var bmpImage = new Bitmap(img);
        var bmpCrop = bmpImage.Clone(cropArea, bmpImage.PixelFormat);
        return bmpCrop;
    }

    public static byte[] imageToByteArray(System.Drawing.Image imageIn)
    {
        MemoryStream ms = new MemoryStream();
        imageIn.Save(ms, System.Drawing.Imaging.ImageFormat.Gif);
        return ms.ToArray();
    }

    public static System.Drawing.Image byteArrayToImage(byte[] byteArrayIn)
    {
        MemoryStream ms = new MemoryStream(byteArrayIn);
        System.Drawing.Image returnImage = System.Drawing.Image.FromStream(ms);
        return returnImage;
    }

    public static void SaveOnNetworkShare(System.Drawing.Image aImage, string aFilename, System.Drawing.Imaging.ImageFormat aImageFormat)
    {
        using (System.IO.MemoryStream lMemoryStream = new System.IO.MemoryStream())
        {
            aImage.Save(lMemoryStream, aImageFormat);

            using (System.IO.FileStream lFileStream = new System.IO.FileStream(aFilename, System.IO.FileMode.Create))
            {
                lMemoryStream.Position = 0;

                lMemoryStream.CopyTo(lFileStream);
            }
        }
    }

    //The actual converting function  
    public static string GetImage(object img)
    {
        return "data:image/jpg;base64," + Convert.ToBase64String((byte[])img);
    }


    public static void PerformImageResizeAndPutOnCanvas(string pFilePath, string pFileName, int pWidth, int pHeight, string pOutputFileName)
    {
        try
        {
            System.Drawing.Image imgBef;
            imgBef = System.Drawing.Image.FromFile(pFilePath + pFileName);


            System.Drawing.Image _imgR;
            _imgR = Imager.Resize(imgBef, pWidth, pHeight, true);


            System.Drawing.Image _img2;
            _img2 = Imager.PutOnWhiteCanvas(_imgR, pWidth, pHeight);

            System.Drawing.Image _imgCrop;
            _imgCrop = Imager.Crop(_img2, new Rectangle(0, 0, 399, 300));

            string path = System.Web.HttpContext.Current.Server.MapPath("~/Media/Thumbs/");
            //Save JPEG  
            Imager.SaveJpeg(path + pOutputFileName.Replace(".jpg.jpg", ".jpg"), _imgCrop);
            //Imager.SaveJpeg(pFilePath + pOutputFileName.Replace(".jpg.jpg",".jpg"), _img2);

        }
        catch (System.IO.FileNotFoundException e)
        {

        }
    }

    public static void PerformImageResizeAndPutOnCanvas(string pFilePath, string pOutputFileName, int pWidth, int pHeight)
    {
        try
        {

            System.Drawing.Image imgBef;
            imgBef = System.Drawing.Image.FromFile(pFilePath);


            System.Drawing.Image _imgR;
            _imgR = Imager.Resize(imgBef, pWidth, pHeight, true);


            System.Drawing.Image _img2;
            _img2 = Imager.PutOnWhiteCanvas(_imgR, pWidth, pHeight);

            System.Drawing.Image _imgCrop;
            _imgCrop = Imager.Crop(_img2, new Rectangle(0, 0, 399, 300));

            string path = System.Web.HttpContext.Current.Server.MapPath("~/Media/Thumbs/");
            //Save JPEG  
            Imager.SaveJpeg(path + pOutputFileName, _imgCrop);
            //Imager.SaveJpeg(pFilePath + pOutputFileName.Replace(".jpg.jpg",".jpg"), _img2);

        }
        catch (System.IO.FileNotFoundException e)
        {

        }

    }
}
#endregion

}