using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace VirusSpreadLibrary.Grid
{
  // source from: github.com/KarlPage/FastGraphics 
  // uses pinned memory and direct array access to manipulate bitmap bits.
  // pinned memory is not owned by the CLR, so, it is important to free when done.
  public sealed class FastBitmap : IDisposable
  {
    private GCHandle pinnedMemoryHandle;
    private IntPtr pinnedMemoryAddress;    

    public int Width { get; private set; }
    public int Height { get; private set; }
    public uint[] Pixels { get; private set; }
    public Bitmap Image { get; private set; }

    public FastBitmap(int width, int height)
    {
      // pixels is an array that may be used to modify pixel data.
      // typical formula is ((y*width)+x) = Alpha | Colour;
      // uses 4 bytes per pixel (Alpha, Red, Green and Blue channels, each 8 bits)
      Pixels = new uint[width * height];

      // inform OS that pixel allocated array is pinned and not subject to normal garbage collection.
      pinnedMemoryHandle = GCHandle.Alloc(Pixels, GCHandleType.Pinned);

      //obtain the pinned memory address, required to create bitmap for use by .NET app.
      pinnedMemoryAddress = pinnedMemoryHandle.AddrOfPinnedObject();

      // set pixel format and calculate scan line stride.
      PixelFormat format = PixelFormat.Format32bppArgb;
      int bitsPerPixel = ((int)format & 0xff00) >> 8;
      int bytesPerPixel = (bitsPerPixel + 7) / 8;
      int stride = 4 * ((width * bytesPerPixel + 3) / 4);
      Image = new Bitmap(width, height, stride, PixelFormat.Format32bppArgb, pinnedMemoryAddress);
      Width = width;
      Height = height;      
    }

    public void Draw(Graphics target) =>
      target.DrawImage(Image, 0, 0);

    public void Dispose()
    {
      Image.Dispose();

      if (pinnedMemoryHandle.IsAllocated)
        pinnedMemoryHandle.Free();

      pinnedMemoryAddress = IntPtr.Zero;
      Pixels = [];
      Width = 0;
      Height = 0;
    }
  }
}
