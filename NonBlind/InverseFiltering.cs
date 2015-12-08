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
        /// <returns></returns>
        public static Image Filtering(Image sourceImage, ConvolutionFilter filter)
        {
            //перевод PSF в частотную область (OTF)
            Complex[,] otf = OpticalTransferFunction.Psf2otf(filter);
            //получение обратного PSF
            for (int u = 0; u < otf.GetLength(0); u++)
                for (int v = 0; v < otf.GetLength(1); v++)
                {
                    otf[u, v] = 1f / otf[u, v];
                }
            //перевод OTF в пространственную область (PSF)   
            ConvolutionFilter cf = OpticalTransferFunction.Otf2psf(otf);
            //свёртка с обратной PSF
            Image result = cf.FastConvolution(sourceImage);

            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sourceImage"></param>
        /// <param name="filter"></param>
        /// <returns></returns>
       /* public static Image Filtering(double[,,] sourceImage, ConvolutionFilter filter)
        {
            Complex[,] otf = OpticalTransferFunction.Psf2otf(filter);
            for (int u = 0; u < otf.GetLength(0); u++)
                for (int v = 0; v < otf.GetLength(1); v++)
                {
                    otf[u, v] = 1f / otf[u, v];
                }
            int filterSize = filter.filterMatrix.GetLength(0);       //размер PSF
            int filterHalfSize = (filterSize - 1) / 2 + 1;                //центр PSF

            ConvolutionFilter cf = OpticalTransferFunction.Otf2psf(otf);

            Image result = cf.FastConvolution(sourceImage);

            return result;
        }
        */

    }
}
