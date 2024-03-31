using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace VRChatLogWathcer.Utils
{
    // https://www.codeproject.com/Articles/5251929/CompactExifLib-Access-to-EXIF-Tags-in-JPEG-TIFF-an

    internal class ExifUtil
    {
        public void SetDateTaken(string path)
        {
            using var pngStream = File.OpenWrite(path);
            var decoder = new PngBitmapDecoder(pngStream, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.Default);
            var frame = decoder.Frames[0];
            var inplaceWriter = frame.CreateInPlaceBitmapMetadataWriter();
            if (inplaceWriter.TrySave())
            {
                inplaceWriter.DateTaken = DateTime.Now.ToString();
                //.SetQuery("/Text/Description", "Have a nice day!");
            }
        }
    }
}
