using ImageEditor;
using System.Numerics;


namespace ImageRecovery
{
    public class TikhonovFiltering
    {
        /// <summary>
        /// Тихоновская регуляризация
        /// </summary>
        /// <param name="filter"> ядро искажения (PSF)</param>
        /// <returns></returns>
        public static ConvolutionFilter Filtering(ConvolutionFilter filter)
        {
            ///в частотной области
            ///fn(u,v)=((h*(u,v)/|h(u,v)|^2+gamma*|p(u,v)|^2))*g(u,v)
            ///fn - приближение
            ///h - kernel
            ///h* - комплексно-сопряженная форма kernel
            ///|h|^2 = h(u,v)*h*(u,v) = u^2+v^2*i
            ///gamma - какой-то параметр (в инверсном фильтре = 0)
            ///p(u,v) = оператор Лапласа = [{0  1  0}
            ///                             {1 -4  1}
            ///                             {0  1  0}]
            ///g - искаженное изображение

            Complex[,] otf = OpticalTransferFunction.Psf2otf(filter);
            int height = otf.GetLength(0);                                              //строк
            int width = otf.GetLength(1);                                              //столбцов
            Complex gamma = Complex.Zero;                                        //
            Complex[,] otfZ = new Complex[height, width];                                   //комплексно сопряженная матрица ядра
            Complex[,] otf2 = new Complex[height, width];                                   //матрица = |h|^2
            Complex[,] p = {{0, 1, 0,},                                          //лапласиан
                           {1, -4, 1,},
                           {0, 1, 0,},};
            p = Fourier.Transform(p);
            for (int u = 0; u < p.GetLength(0); u++)
                for (int v = 0; v < p.GetLength(1); v++)
                    p[u, v] = OpticalTransferFunction.ModPow(p[u, v]);

            for (int u = 0; u < height; u++)
                for (int v = 0; v < width; v++)
                    otfZ[u, v] = Complex.Conjugate(otf[u, v]);

            for (int u = 0; u < height; u++)
                for (int v = 0; v < width; v++)
                    otf2[u, v] = OpticalTransferFunction.ModPow(otf[u, v]);

            for (int u = 0; u < height; u++)
                for (int v = 0; v < width; v++)
                    p[u, v] = p[u, v] * gamma;

            for (int u = 0; u < height; u++)
                for (int v = 0; v < width; v++)
                    otf2[u, v] = otf2[u, v] + p[u, v];


            for (int u = 0; u < height; u++)
                for (int v = 0; v < width; v++)
                    otf[u, v] = otfZ[u, v] / otf2[u, v];

            ConvolutionFilter cf = OpticalTransferFunction.Otf2psf(otf);

            return cf;
        }
    }
}
