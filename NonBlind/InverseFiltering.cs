using ImageEditor;
using System.Numerics;
using System.Drawing;

namespace ImageRecovery
{
    public class InverseFiltering
    {
        /// <summary>
        /// Инверсная фильтрация
        /// </summary>
        /// <param name="sourceImage"> искаженное изображение</param>
        /// <param name="filter"> оператор искажения PSF</param>
        /// <param name="outfilter">восстанавливающий фильтр</param>
        /// <returns></returns>
        public static Image Filtering(Image sourceImage, ConvolutionFilter filter, out ConvolutionFilter outfilter)
        {
            //перевод PSF в частотную область (OTF)
            Complex[,] otf = OpticalTransferFunction.Psf2otf(filter);
            //получение обратного PSF
            for (int u = 0; u < otf.GetLength(0); u++)
                for (int v = 0; v < otf.GetLength(1); v++)
                {
                    otf[u, v] = 1f / otf[u, v];
                }
            //перевод OTF обратно в пространственную область (PSF)   
            outfilter = OpticalTransferFunction.Otf2psf(otf);
            //быстрая свёртка изображения с обратной PSF
            Image result = outfilter.FastConvolution(sourceImage);

            return result;
        }
    }
}
