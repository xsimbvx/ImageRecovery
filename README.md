# ImageRecovery
Библиотека алгоритмов реконструкции расфокусированных изображений.

## Опсание
Библиотека была разработана как часть дипломной работы. Она включает в себя ещё три части: [Легковесная библиотека для работы с изображениями на .NET](https://github.com/Kovnir/ImageEditor), [Модифицированный алгоритм восстановления изображений](https://github.com/Kovnir/DeblurModification), [Пример интеграции с библиотеками](https://github.com/xsimbvx/IRIntegration). Сборка проекта производилась в Microsoft Visual Studio 2015.


### Инверсная фильтрация
Любое смазанное изображение является результатом свёртки (конволюции от англ. *convolution*) исходного (чёткого изображения) и оператора искажения PSF (Point Spread Function).
Операции свёртки в пространственной области эквивалентна операция обратного Фурье-преобразования поэлементного умножения Фурье-образов этих двух функций.


Для того, чтобы получить исходное изображения имея расфокусированное изображение и информацию об искажении (PSF) 
необходимо получить обратную PSF. Затем выполнить свёртку размытого изображения с обратной PSF.

### Пример1
```c#
image2 = InverseFiltering.Filtering(image1, psf);
```
image1 — искаженное изображение типа Image, psf — оператор искажения типа [ConvolutionFilter](https://github.com/Kovnir/ImageEditor/blob/master/Tools/ConvolutionFilter.cs).

### Пример2
```c#
image2 = InverseFiltering.Filtering(image1, psf);
```
image1 — трехмерная матрица типа double содержащая RGB-каналы изображения, psf — оператор искажения типа [ConvolutionFilter](https://github.com/Kovnir/ImageEditor/blob/master/Tools/ConvolutionFilter.cs).
