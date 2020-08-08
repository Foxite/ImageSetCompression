# ImageSetCompression
A .NET Standard library for compressing sets of similar images. There's a .NET Core console app and a Xamarin.Android app that use the library.

It uses [SixLabors.ImageSharp](https://github.com/SixLabors/ImageSharp) for image operations, as System.Drawing does not run on anything other than Windows despite claiming to be .NET Standard.

The library currently only supports a very basic pixel-based method for comparing the images, which takes a "base" image and saves "delta" images in which each pixel corresponds to the RGB difference between the base and variant images. Support for one or more real compression algorithms is planned pending access to the research papers.
