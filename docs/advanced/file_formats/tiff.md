# TIFF

Tagged Image File Format (TIFF) is an image file format originally created by Aldus. 
Currently there is no metadata stored by N.I.N.A. for this specific format.

## TIFF raw

This format stores the image in its pure raw format without using any compression. This type is the fastest, but also largest in size.

## TIFF compressed

TIFF is capable of storing the image in a lossless compressed format. 
This will reduce the file size considerable without altering the image in any way, but the downside is increased processing time when saving and loading the image.
Some software might also not be compatible with compressed TIFF files.

### zip vs lzw

Zip and lzw are different algorithms for compression and the result is highly depending on the input data. 
It could even be possible that the file size increases when the data is laid out in a worst case scenario for the algorithm.
For deciding on an algorithm it is advised to try out which one works best for your specific camera and data.
