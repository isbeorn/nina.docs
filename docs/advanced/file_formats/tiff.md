# TIFF

Tagged Image File Format (TIFF) is an image file format originally created by Aldus. 
N.I.N.A. stores a text representation of its FITS-style header cards in the TIFF title metadata. This preserves part of the capture metadata for software that reads that field, but TIFF does not provide the same astronomy-specific interoperability as FITS or XISF.

## TIFF raw

This format stores the image data without TIFF compression. It is normally the fastest option, but also produces the largest files.

## TIFF compressed

TIFF is capable of storing the image in a lossless compressed format. 
This can reduce the file size without altering the image data, but the downside is increased processing time when saving and loading the image.
Some software might also not be compatible with compressed TIFF files.

### zip vs lzw

ZIP and LZW are lossless compression algorithms whose results depend on the input data. A worst-case image can even become slightly larger. Test both with representative frames from the intended camera and processing software.
