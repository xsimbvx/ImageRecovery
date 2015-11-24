using ImageEditor;
using System.Numerics;
using ImageRecovery.Tools;
using System.Drawing;


namespace ImageRecovery.NonBlind
{
    public class InverseFiltering
    {
        /// <summary>
        /// Инверсная фильтрация
        /// </summary>
        /// <param name="filter">ядро искажения (PSF)</param>
        /// <returns></returns>
        public static Image Filtering(Image sourceImage, ConvolutionFilter filter, bool interpolation)
        {
            Complex[,] otf = OpticalTransferFunction.Psf2otf(filter);
            for (int u = 0; u < otf.GetLength(0); u++)
                for (int v = 0; v < otf.GetLength(1); v++)
                {      
                    otf[u,v] = 1f / otf[u,v];
                }
            ConvolutionFilter cf = OpticalTransferFunction.Otf2psf(otf);
            int filterSize = filter.filterMatrix.GetLength(0);       //размер PSF
            int filterHalfSize = (filterSize - 1) / 2 + 1;                //центр PSF
            Image expandedImage = sourceImage.Expand(filterHalfSize);
            Image image = expandedImage;
            if(interpolation)
            {
                double[,,] expImageArray = OpticalTransferFunction.Interpolation(Converter.ToDoubleArray(expandedImage), expandedImage.Height, expandedImage.Width);
                image = OpticalTransferFunction.FourierConvolution(expImageArray, cf);
            }
            else
                image = OpticalTransferFunction.FourierConvolution(expandedImage, cf);

            return image;
        }
      
    }
}
