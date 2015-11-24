using ImageEditor;
using System.Numerics;
using System.Drawing;
using ImageRecovery.Tools;

namespace ImageRecovery.NonBlind
{
    public class WienerFiltering
    {
        /// <summary>
        /// Винеровская фильтрация
        /// </summary>
        /// <param name="sourceImage">Неискаженное изображение</param>
        /// <param name="filter">Фильтр, который был наложен на изображение</param>
        /// <param name="noise">Аддитивный шум, который был наложен на изображение</param>
        /// <returns></returns>
        // public static Image Filtering(Image f, Image g, ConvolutionFilter h, byte[,] n, out ConvolutionFilter outFilter)
        public static ConvolutionFilter Filtering(ConvolutionFilter filter, Image sourceImage, byte[,] noise)
        {

            Complex[,] otf = OpticalTransferFunction.Psf2otf(filter);
            int filterSize = otf.GetLength(0);

            Complex[,] otf2 = new Complex[filterSize, filterSize];
            for (int u = 0; u < filterSize; u++)
                for (int v = 0; v < filterSize; v++)
                    otf2[u, v] = OpticalTransferFunction.ModPow(otf[u, v]);

            Complex[,] otfZ = new Complex[filterSize, filterSize];
            for (int u = 0; u < filterSize; u++)
                for (int v = 0; v < filterSize; v++)
                    otfZ[u, v] = Complex.Conjugate(otf[u, v]);

            Complex[,] imageFourier = Fourier.Transform(Converter.ToComplexMatrix(sourceImage));
            Complex[,] noiseFourier = Fourier.Transform(Converter.ToComplexMatrix(noise));
            int height = sourceImage.Height;
            int width = sourceImage.Width;
            Complex[,] noiseSpectrum = new Complex[height, width];         //энергетический спектр шума
            Complex[,] imageSpectrum = new Complex[height, width];          //спектр неискаженного сигнала
            Complex noiseEnergy = new Complex(0, 0);                                         //средняя энергия шума
            Complex imageEnergy = new Complex(0, 0);                                         //средняя энергия изображения
            Complex R = new Complex(0, 0);                                                     //константа вместо Sn(u,v)/Sf(u,v)
            for (int u = 0; u < height; u++)
                for (int v = 0; v < width; v++)
                {
                    noiseEnergy += OpticalTransferFunction.ModPow(noiseFourier[u, v]);
                    imageEnergy += OpticalTransferFunction.ModPow(imageFourier[u, v]);
                }

            noiseEnergy /= height * width;
            imageEnergy /= height * width;
            R = noiseEnergy / imageEnergy;


            Complex[,] resultPSF = new Complex[filterSize, filterSize];
            for (int u = 0; u < filterSize; u++)
                for (int v = 0; v < filterSize; v++)
                    resultPSF[u, v] = (otfZ[u, v] / (otf2[u, v]) + R);
            ConvolutionFilter cf = OpticalTransferFunction.Otf2psf(resultPSF);
            return cf;
        }
    }
}
