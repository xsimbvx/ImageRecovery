using ImageEditor;
using System.Numerics;
using System.Drawing;
using System;

namespace ImageRecovery
{
    public class WienerFiltering
    {
        /// <summary>
        /// Винеровская фильтрация
        /// </summary>
        /// <param name="sourceImage">исходное изображение</param>
        /// <param name="bluredImage">искаженное изображение</param>
        /// <param name="filter">PSF</param>
        /// <param name="noise">Шум</param>
        /// <returns></returns>
        public static Image Filtering(Image sourceImage, Image bluredImage, ConvolutionFilter filter, byte[,] noise)
        {
            int height = sourceImage.Height;
            int width = sourceImage.Width;
            Complex[,] otf = OpticalTransferFunction.Psf2otf(filter);
            for (int u = 0; u < otf.GetLength(0); u++)
                for (int v = 0; v < otf.GetLength(1); v++)
                {
                    otf[u, v] = 1f / otf[u, v];
                }
            int filterSize = filter.filterMatrix.GetLength(0);       //размер PSF
            int filterHalfSize = (filterSize - 1) / 2 + 1;                //центр PSF
            ConvolutionFilter cf = OpticalTransferFunction.Otf2psf(otf);

            Image expandedBluredImage = bluredImage.Expand(filterHalfSize);
            Image expandedSourceImage = sourceImage.Expand(filterHalfSize);
            int expHeight = expandedSourceImage.Height;
            int expWidth = expandedSourceImage.Width;
            double[] expImage = Converter.ToDoubleArray(expandedBluredImage);
            double[] expSourceImage = Converter.ToDoubleArray(expandedSourceImage);
            double[,] bluredRed = new double[expHeight, expWidth];
            double[,] bluredGreen = new double[expHeight, expWidth];
            double[,] bluredBlue = new double[expHeight, expWidth];
            double[,] sourceRed = new double[expHeight, expWidth];
            double[,] sourceGreen = new double[expHeight, expWidth];
            double[,] sourceBlue = new double[expHeight, expWidth];

            int index = 0;
            for (int i = 0; i < expHeight; i++)
                for (int j = 0; j < expWidth; j++)
                {
                    bluredBlue[i, j] = expImage[index];
                    bluredGreen[i, j] = expImage[index + 1];
                    bluredRed[i, j] = expImage[index + 2];
                    sourceBlue[i, j] = expSourceImage[index];
                    sourceGreen[i, j] = expSourceImage[index + 1];
                    sourceRed[i, j] = expSourceImage[index + 2];
                    index += 4;
                }

            //перевод в частотную область
            //искаженное изображение
            Complex[,] bluredRedFourier = Fourier.FastTransform(Converter.ToComplexMatrix(bluredRed));
            Complex[,] bluredGreenFourier = Fourier.FastTransform(Converter.ToComplexMatrix(bluredGreen));
            Complex[,] bluredBlueFourier = Fourier.FastTransform(Converter.ToComplexMatrix(bluredBlue));
            //исходное изображение
            Complex[,] redSpectrum = Fourier.FastTransform(Converter.ToComplexMatrix(sourceRed));
            Complex[,] greenSpectrum = Fourier.FastTransform(Converter.ToComplexMatrix(sourceGreen));
            Complex[,] blueSpectrum = Fourier.FastTransform(Converter.ToComplexMatrix(sourceBlue));
            //шум и PSF
            Complex[,] noiseSpectrum = Fourier.FastTransform(Converter.ToComplexMatrix(noise));
            int newSize = bluredBlueFourier.GetLength(0);
            double[,] kernel_1 = cf.ExpendedByZero(newSize);
            Complex[,] kernel_1Fourier = Fourier.FastTransform(Converter.ToComplexMatrix(kernel_1));
            double[,] kernel = filter.ExpendedByZero(newSize);
            Complex[,] kernelFourier = Fourier.FastTransform(Converter.ToComplexMatrix(kernel));
            Complex[,] kernelFourierPow = new Complex[newSize, newSize];////добавить функцию
            for (int u = 0; u < newSize; u++)
                for (int v = 0; v < newSize; v++)
                {
                    kernelFourierPow[u, v] = OpticalTransferFunction.ModPow(kernelFourier[u, v]);

                    bluredRedFourier[u, v] *= ((kernel_1Fourier[u, v]) * (kernelFourierPow[u, v] / (kernelFourierPow[u, v] + noiseSpectrum[u, v] / redSpectrum[u, v])));
                    bluredGreenFourier[u, v] *= ((kernel_1Fourier[u, v]) * (kernelFourierPow[u, v] / (kernelFourierPow[u, v] + noiseSpectrum[u, v] / greenSpectrum[u, v])));
                    bluredBlueFourier[u, v] *= ((kernel_1Fourier[u, v]) * (kernelFourierPow[u, v] / (kernelFourierPow[u, v] + noiseSpectrum[u, v] / blueSpectrum[u, v])));
                }
            Complex[,] newRed = Fourier.IFastTransform(bluredRedFourier, expHeight, expWidth);
            Complex[,] newGreen = Fourier.IFastTransform(bluredGreenFourier, expHeight, expWidth);
            Complex[,] newBlue = Fourier.IFastTransform(bluredBlueFourier, expHeight, expWidth);

            Complex[,] resRed = new Complex[height, width];
            Complex[,] resGreen = new Complex[height, width];
            Complex[,] resBlue = new Complex[height, width];

            int resultSize = height * width * 4;
            Complex[] resultImage = new Complex[resultSize];
            index = 0;
            for (int i = 0; i < height; i++)
                for (int j = 0; j < width; j++)
                {
                    resRed[i, j] = Math.Round(newRed[i + filterHalfSize + 1, j + filterHalfSize + 1].Real);
                    resGreen[i, j] = Math.Round(newGreen[i + filterHalfSize + 1, j + filterHalfSize + 1].Real);
                    resBlue[i, j] = Math.Round(newBlue[i + filterHalfSize + 1, j + filterHalfSize + 1].Real);
                }
            Image result = Converter.ToImage(resRed, resGreen, resBlue);

            return result;
        }
    }
}