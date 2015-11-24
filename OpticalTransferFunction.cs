using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using ImageEditor;
using System.Drawing;

namespace ImageRecovery.Tools
{
    class OpticalTransferFunction
    {

        /// <summary>
        /// Возведение в квадрат модуля косплексного числа
        /// </summary>
        /// <param name="val">Комплексное число</param>
        /// <returns></returns>
        public static Complex ModPow(Complex val)
        {
            return new Complex(Math.Pow(val.Real, 2), Math.Pow(val.Imaginary, 2));
        }
        /// <summary>
        /// Перевод из PSF(Point Spread Function) в OTF (Optical Transform Function) того же размера
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        public static Complex[,] Psf2otf(ConvolutionFilter filter)
        {
            double[,] filterMatrix = filter.normalizedFilterMatrix;
            int FilterSize = filterMatrix.GetLength(0);
            int halfSize = (FilterSize - 1) / 2;
            int ost = FilterSize - halfSize;
            double[,] newFilter = new double[FilterSize, FilterSize];

            //+ + -
            //+ + -
            //- - -
            for (int i = 0; i < ost; i++)
                for (int j = 0; j < ost; j++)
                    newFilter[i, j] = filterMatrix[i + halfSize, j + halfSize];
            //- - +
            //- - +
            //- - -
            for (int i = 0; i < ost; i++)
                for (int j = ost; j < FilterSize; j++)
                    newFilter[i, j] = filterMatrix[i + halfSize, j - ost];
            //- - -
            //- - -
            //+ + -
            for (int i = ost; i < FilterSize; i++)
                for (int j = 0; j < ost; j++)
                    newFilter[i, j] = filterMatrix[i - ost, j + halfSize];
            //- - -
            //- - -
            //- - +
            for (int i = ost; i < FilterSize; i++)
                for (int j = ost; j < FilterSize; j++)
                    newFilter[i, j] = filterMatrix[i - ost, j - ost];

            return Fourier.Transform(Converter.ToComplexMatrix(newFilter));

        }//+

        /// <summary>
        /// Перевод из PSF(Point Spread Function) в OTF (Optical Transform Function) с заданным (большим) размером 
        /// </summary>
        /// <param name="filter">Исходный фильтр</param>
        /// <param name="newHeight">Новое количество пикселов в высоту</param>
        /// <param name="newWidth">Новое количество пикселов в ширину</param>
        /// <returns></returns>
        public static Complex[,] Psf2otf(ConvolutionFilter filter, int newHeight, int newWidth)
        {
            double[,] filterMatrix = filter.normalizedFilterMatrix;
            int sourceFilterSize = filterMatrix.GetLength(0);
            int halfSize = (filter.filterMatrix.GetLength(0) - 1) / 2;
            if (newHeight < sourceFilterSize || newWidth < sourceFilterSize)
                return null;
            double[,] extendedFilter = new double[newHeight, newWidth];
            //0 0 0
            //0 0 0
            //0 0 0
            for (int i = 0; i < newHeight; i++)
                for (int j = 0; j < newWidth; j++)
                {
                    extendedFilter[i, j] = 0;
                }
            //- - -
            //- + +
            //- + +
            for (int i = 0; i < halfSize + 1; i++)
                for (int j = 0; j < halfSize + 1; j++)
                    extendedFilter[i, j] = filterMatrix[i + halfSize, j + halfSize];
            //- - -
            //+ - -
            //+ - -
            for (int i = 0; i < halfSize + 1; i++)
                for (int j = newWidth - halfSize; j < newWidth; j++)
                    extendedFilter[i, j] = filterMatrix[i + halfSize, j - (newWidth - halfSize)];
            //- + +
            //- - -
            //- - -
            for (int i = newHeight - halfSize; i < newHeight; i++)
                for (int j = 0; j < halfSize + 1; j++)
                    extendedFilter[i, j] = filterMatrix[i - (newHeight - halfSize), j + halfSize];
            //+ - -
            //- - -
            //- - -
            for (int i = newHeight - halfSize; i < newHeight; i++)
                for (int j = newWidth - halfSize; j < newWidth; j++)
                    extendedFilter[i, j] = filterMatrix[i - (newHeight - halfSize), j - (newWidth - halfSize)];

            return Fourier.Transform(Converter.ToComplexMatrix(extendedFilter));
        }//+

        /// <summary>
        /// Перевод из OTF(Optical Transform Function) в PSF(Point Spread Function) с заданным размером 
        /// </summary>
        /// <param name="otf"></param>
        /// <returns></returns>
        public static ConvolutionFilter Otf2psf(Complex[,] otf)
        {
            Complex[,] psf = Fourier.ITransform(otf);
            int FilterSize = psf.GetLength(0);
            int halfSize = (FilterSize - 1) / 2;
            int ost = FilterSize - halfSize;
            Complex[,] returnPSF = new Complex[FilterSize, FilterSize];
            //+ - -
            //- - -
            //- - -
            for (int i = 0; i < halfSize; i++)
                for (int j = 0; j < halfSize; j++)
                    returnPSF[i, j] = psf[i + ost, j + ost];
            //- + +
            //- - -
            //- - -
            for (int i = 0; i < halfSize; i++)
                for (int j = halfSize; j < FilterSize; j++)
                    returnPSF[i, j] = psf[i + ost, j - halfSize];
            //- - -
            //+ - -
            //+ - -
            for (int i = halfSize; i < FilterSize; i++)
                for (int j = 0; j < halfSize; j++)
                    returnPSF[i, j] = psf[i - halfSize, j + ost];
            //- - -
            //- + +
            //- + +
            for (int i = halfSize; i < FilterSize; i++)
                for (int j = halfSize; j < FilterSize; j++)
                    returnPSF[i, j] = psf[i - halfSize, j - halfSize];

            ConvolutionFilter cf = new ConvolutionFilter("Recovery Fiter", Converter.ToDoubleMatrix(returnPSF));
            return cf;
        }//+

        public static Complex[,] clipChannel(Complex[,] channel, int offset)
        {

            int channelSize = channel.GetLength(0);
            Complex[,] newChannel = new Complex[channelSize - offset * 2, channelSize - offset * 2];
            for (int i = offset; i < channelSize - offset; i++)
                for (int j = offset; j < channelSize - offset; j++)
                    newChannel[i - offset, j - offset] = channel[i, j];
            return newChannel;
        }


        public static Complex[,] ExpendChannel(Complex[,] channel, int offset)
        {

            int channelSize = channel.GetLength(0);
            Complex[,] newChannel = new Complex[channelSize + offset, channelSize + offset];
            for (int i = 0; i < newChannel.GetLength(0); i++)
                for (int j = 0; j < newChannel.GetLength(0); j++)
                    newChannel[i, j] = new Complex(0, 0);
            for (int i = 0; i < channelSize; i++)
                for (int j = 0; j < channelSize; j++)
                {
                    newChannel[i, j] = channel[i, j];
                }

            return newChannel;
        }

        public static double[,] ExpendedByZero(double[,] data, int newSize)
        {
            double[,] newData = new double[newSize, newSize];
            for (int i = 0; i < newSize; i++)
                for (int j = 0; j < newSize; j++)
                    newData[i, j] = 0;

            for (int i = 0; i < data.GetLength(0); i++)
                for (int j = 0; j < data.GetLength(1); j++)
                    newData[i, j] = data[i, j];
            return newData;
        }

        public static Complex[,] ReturnImage(Complex[,] data, int newHeight, int newWidth)
        {
            Complex[,] newData = new Complex[newHeight, newWidth];
            for (int i = 0; i < newHeight; i++)
                for (int j = 0; j < newWidth; j++)
                    newData[i, j] = data[i + 1, j + 1];
            return newData;
        }


        /// <summary>
        /// Быстрая свёртка
        /// </summary>
        /// <param name="sourceImage"></param>
        /// <param name="filter"></param>
        /// <param name="image"></param>
        /// <returns></returns>
        public static Image FourierConvolution(Image sourceImage, ConvolutionFilter filter)
        {
            int height = sourceImage.Height;
            int width = sourceImage.Width;
            int filterSize = filter.filterMatrix.GetLength(0);       //размер PSF
            int filterHalfSize = (filterSize - 1) / 2 + 1;                //центр PSF
            double[] image = Converter.ToDoubleArray(sourceImage);
            double[,] red = new double[height, width];
            double[,] green = new double[height, width];
            double[,] blue = new double[height, width];
            int index = 0;
            for (int i = 0; i < height; i++)
                for (int j = 0; j < width; j++)
                {
                    blue[i, j] = image[index];
                    green[i, j] = image[index + 1];
                    red[i, j] = image[index + 2];
                    index += 4;
                }
            int newSize = 0;
            if (height > width)
                newSize = height;
            newSize = width;
            newSize += filterSize;
            if (newSize % 2 == 0)
                newSize++;
            double[,] extRed = OpticalTransferFunction.ExpendedByZero(red, newSize);
            double[,] extGreen = OpticalTransferFunction.ExpendedByZero(green, newSize);
            double[,] extBlue = OpticalTransferFunction.ExpendedByZero(blue, newSize);
            double[,] kernel = OpticalTransferFunction.ExpendedByZero(filter.normalizedFilterMatrix, newSize);

            Complex[,] redFourier = Fourier.Transform(Converter.ToComplexMatrix(extRed));
            Complex[,] greenFourier = Fourier.Transform(Converter.ToComplexMatrix(extGreen));
            Complex[,] blueFourier = Fourier.Transform(Converter.ToComplexMatrix(extBlue));
            Complex[,] kernelFourier = Fourier.Transform(Converter.ToComplexMatrix(kernel));
            for (int u = 0; u < newSize; u++)
                for (int v = 0; v < newSize; v++)
                {
                    redFourier[u, v] *= kernelFourier[u, v];
                    greenFourier[u, v] *= kernelFourier[u, v];
                    blueFourier[u, v] *= kernelFourier[u, v];
                }
            Complex[,] newRed = Fourier.ITransform(redFourier);
            Complex[,] newGreen = Fourier.ITransform(greenFourier);
            Complex[,] newBlue = Fourier.ITransform(blueFourier);

            Complex[,] resRed = OpticalTransferFunction.ReturnImage(newRed, height, width);//OpticalTransferFunction.clipChannel(newRed, filterHalfSize);
            Complex[,] resGreen = OpticalTransferFunction.ReturnImage(newGreen, height, width);//OpticalTransferFunction.clipChannel(newGreen, filterHalfSize);
            Complex[,] resBlue = OpticalTransferFunction.ReturnImage(newBlue, height, width);//OpticalTransferFunction.clipChannel(newBlue, filterHalfSize);

            int resultSize = resBlue.GetLength(0) * resBlue.GetLength(1) * 4;
            Complex[] resultImage = new Complex[resultSize];
            index = 0;
            for (int i = 0; i < height; i++)
                for (int j = 0; j < width; j++)
                {
                    resultImage[index] = resBlue[i, j];
                    resultImage[index + 1] = resGreen[i, j];
                    resultImage[index + 2] = resRed[i, j];
                    resultImage[index + 3] = new Complex(255, 0);
                    index += 4;
                }

            ////
            int newResultSize = (sourceImage.Height-filterHalfSize*2) * (sourceImage.Width-filterHalfSize*2) * 4;
            Complex[] newResult = new Complex[newResultSize];
            index = 0;
            for (int i = 0; i < height; i++)
                for (int j = 0; j < width; j++)
                {
                    blue[i, j] = Math.Round(resultImage[index].Real);
                    green[i, j] = Math.Round(resultImage[index+1].Real);
                    red[i, j] = Math.Round(resultImage[index+2].Real);
                    index += 4;
                }
            index = 0;
            for (int i = filterHalfSize; i < height - filterHalfSize; i++)
                for (int j = filterHalfSize; j < width - filterHalfSize; j++)
                {
                    newResult[index] = blue[i, j];
                    newResult[index + 1] = green[i, j];
                    newResult[index + 2] = red[i, j];
                    newResult[index + 3] = 255;
                    index += 4;
                }
            Image result = Converter.ToImage(newResult, width-2*filterHalfSize);
            return result;
        }
        public static Image FourierConvolution(double[,,] sourceImage, ConvolutionFilter filter)
        {
            int height = sourceImage.GetLength(0);
            int width = sourceImage.GetLength(1);
            int filterSize = filter.filterMatrix.GetLength(0);       //размер PSF
            int filterHalfSize = (filterSize - 1) / 2 + 1;                //центр PSF
            double[,] red = new double[height, width];
            double[,] green = new double[height, width];
            double[,] blue = new double[height, width];
           
            for (int i = 0; i < height; i++)
                for (int j = 0; j < width; j++)
                {
                    blue[i, j] = sourceImage[i,j,0];
                    green[i, j] = sourceImage[i, j, 1];
                    red[i, j] = sourceImage[i, j, 2];
                }
            int newSize = 0;
            if (height > width)
                newSize = height;
            newSize = width;
            newSize += filterSize;
            if (newSize % 2 == 0)
                newSize++;
            double[,] extRed = OpticalTransferFunction.ExpendedByZero(red, newSize);
            double[,] extGreen = OpticalTransferFunction.ExpendedByZero(green, newSize);
            double[,] extBlue = OpticalTransferFunction.ExpendedByZero(blue, newSize);
            double[,] kernel = OpticalTransferFunction.ExpendedByZero(filter.normalizedFilterMatrix, newSize);

            Complex[,] redFourier = Fourier.Transform(Converter.ToComplexMatrix(extRed));
            Complex[,] greenFourier = Fourier.Transform(Converter.ToComplexMatrix(extGreen));
            Complex[,] blueFourier = Fourier.Transform(Converter.ToComplexMatrix(extBlue));
            Complex[,] kernelFourier = Fourier.Transform(Converter.ToComplexMatrix(kernel));
            for (int u = 0; u < newSize; u++)
                for (int v = 0; v < newSize; v++)
                {
                    redFourier[u, v] *= kernelFourier[u, v];
                    greenFourier[u, v] *= kernelFourier[u, v];
                    blueFourier[u, v] *= kernelFourier[u, v];
                }
            Complex[,] newRed = Fourier.ITransform(redFourier);
            Complex[,] newGreen = Fourier.ITransform(greenFourier);
            Complex[,] newBlue = Fourier.ITransform(blueFourier);

            Complex[,] resRed = OpticalTransferFunction.ReturnImage(newRed, height, width);//OpticalTransferFunction.clipChannel(newRed, filterHalfSize);
            Complex[,] resGreen = OpticalTransferFunction.ReturnImage(newGreen, height, width);//OpticalTransferFunction.clipChannel(newGreen, filterHalfSize);
            Complex[,] resBlue = OpticalTransferFunction.ReturnImage(newBlue, height, width);//OpticalTransferFunction.clipChannel(newBlue, filterHalfSize);

            int resultSize = resBlue.GetLength(0) * resBlue.GetLength(1) * 4;
            Complex[] resultImage = new Complex[resultSize];
            int index = 0;
            for (int i = 0; i < height; i++)
                for (int j = 0; j < width; j++)
                {
                    resultImage[index] = resBlue[i, j];
                    resultImage[index + 1] = resGreen[i, j];
                    resultImage[index + 2] = resRed[i, j];
                    resultImage[index + 3] = new Complex(255, 0);
                    index += 4;
                }

            ////
            int newResultSize = (height - filterHalfSize * 2) * (width - filterHalfSize * 2) * 4;
            Complex[] newResult = new Complex[newResultSize];
            index = 0;
            for (int i = 0; i < height; i++)
                for (int j = 0; j < width; j++)
                {
                    blue[i, j] = Math.Round(resultImage[index].Real);
                    green[i, j] = Math.Round(resultImage[index + 1].Real);
                    red[i, j] = Math.Round(resultImage[index + 2].Real);
                    index += 4;
                }
            index = 0;
            for (int i = filterHalfSize; i < height - filterHalfSize; i++)
                for (int j = filterHalfSize; j < width - filterHalfSize; j++)
                {
                    newResult[index] = blue[i, j];
                    newResult[index + 1] = green[i, j];
                    newResult[index + 2] = red[i, j];
                    newResult[index + 3] = 255;
                    index += 4;
                }

            ////


            Image result = Converter.ToImage(newResult, width - 2 * filterHalfSize);
            return result;
        }



        public static double[,] FractionalPartRestoration(double[,] data)
        {
            int height = data.GetLength(0);
            int width = data.GetLength(1);
            double[,] newData = new double[height, width];
            double[,] newHorizontalData = data.Clone() as double[,];
            double[,] newVerticalData = data.Clone() as double[,];
            Func<double, double, double, double> Interpolate = (pre, current, next) =>
                {
                    double newValue = (pre + next) / 2;

                    if (newValue >= current + 0.5)
                        return current + 0.4999999;
                    if (newValue < current - 0.5)
                        return current - 0.5;
                    return newValue;
                };

            for (int i = 0; i < height; i++)
                for (int j = 1; j < width - 1; j++)
                    newHorizontalData[i, j] = Interpolate(newHorizontalData[i, j - 1], newHorizontalData[i, j], newHorizontalData[i, j + 1]);

            for (int i = 1; i < height - 1; i++)
                for (int j = 0; j < width; j++)
                    newVerticalData[i, j] = Interpolate(newVerticalData[i - 1, j], newVerticalData[i, j], newVerticalData[i + 1, j]);

            for (int i = 0; i < height; i++)
                for (int j = 0; j < width; j++)
                    newData[i, j] = (newVerticalData[i, j] + newHorizontalData[i, j]) / 2;

            return newData;
        }

        public static double[,,] FractionalPartRestoration(double[, ,] data)
        {
            double[,] blue = new double[data.GetLength(0), data.GetLength(1)];
            double[,] green = new double[data.GetLength(0), data.GetLength(1)];
            double[,] red = new double[data.GetLength(0), data.GetLength(1)];
            for (int i = 0; i < data.GetLength(0); i++)
                for (int j = 0; j < data.GetLength(1); j++)
                {
                    blue[i, j] = data[i, j, 0];
                    green[i, j] = data[i, j, 1];
                    red[i, j] = data[i, j, 2];
                }
            blue = FractionalPartRestoration(blue);
            green = FractionalPartRestoration(green);
            red = FractionalPartRestoration(red);
            for (int i = 0; i < data.GetLength(0); i++)
                for (int j = 0; j < data.GetLength(1); j++)
                {
                    data[i, j, 0] = blue[i, j];
                    data[i, j, 1] = green[i, j];
                    data[i, j, 2] = red[i, j];
                }
            return data;
            
        }


        
            public static double[,,] Interpolation(double[] data,int height, int width)
            {
                double[, ,] interpolateMatrix = new double[height, width, 3];
            int index = 0;
            for (int i = 0; i < height; i++)
                for (int j = 0; j < width; j++)
                {
                    interpolateMatrix[i, j, 0] = data[index + 0];//blue channel
                    interpolateMatrix[i, j, 1] = data[index + 1];//green channel
                    interpolateMatrix[i, j, 2] = data[index + 2];//red channel
                    index += 4;
                }
            return OpticalTransferFunction.FractionalPartRestoration(interpolateMatrix);
                       
            }

    }
}
