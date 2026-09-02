using System;
using System.Drawing;
using System.Runtime.InteropServices;

namespace Ule4JisAlt
{
    public static class IconGenerator
    {
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool DestroyIcon(IntPtr handle);

        public static Icon CreatePixelKeyIcon(bool enabled)
        {
            using Bitmap bmp = new Bitmap(16, 16);

            // 色定義
            Color cTransparent = Color.Transparent;
            Color cOuterBorder = Color.FromArgb(30, 30, 30); // 外枠の黒ドット

            Color cKeyFace;
            Color cHighlight;
            Color cShadow;
            Color cText;

            if (enabled)
            {
                // ON状態：レトロ・クラシックブルーキーキャップ
                cKeyFace = Color.FromArgb(50, 120, 200);
                cHighlight = Color.FromArgb(130, 190, 255);
                cShadow = Color.FromArgb(25, 70, 130);
                cText = Color.FromArgb(255, 255, 255);
            }
            else
            {
                // OFF状態：添付画像のようなシックなピクセルグレーキーキャップ
                cKeyFace = Color.FromArgb(70, 75, 80);
                cHighlight = Color.FromArgb(120, 125, 130);
                cShadow = Color.FromArgb(40, 42, 45);
                cText = Color.FromArgb(140, 145, 150);
            }

            // 全体をキー面の色で塗りつぶし
            for (int x = 0; x < 16; x++)
            {
                for (int y = 0; y < 16; y++)
                {
                    bmp.SetPixel(x, y, cKeyFace);
                }
            }

            // 四隅のクリア
            bmp.SetPixel(0, 0, cTransparent);
            bmp.SetPixel(0, 15, cTransparent);
            bmp.SetPixel(15, 0, cTransparent);
            bmp.SetPixel(15, 15, cTransparent);

            // 外枠（黒ドット）
            for (int i = 1; i < 15; i++)
            {
                bmp.SetPixel(i, 0, cOuterBorder);
                bmp.SetPixel(i, 15, cOuterBorder);
                bmp.SetPixel(0, i, cOuterBorder);
                bmp.SetPixel(15, i, cOuterBorder);
            }

            // ハイライト（左・上）
            for (int i = 1; i < 14; i++)
            {
                bmp.SetPixel(i, 1, cHighlight);
                bmp.SetPixel(1, i, cHighlight);
            }

            // シャドウ（右・下）
            for (int i = 1; i < 15; i++)
            {
                bmp.SetPixel(i, 14, cShadow);
                bmp.SetPixel(14, i, cShadow);
            }

            // ピクセルアートの「U」文字描画 (5x6 ドット)
            // 縦棒（左）
            bmp.SetPixel(5, 5, cText); bmp.SetPixel(6, 5, cText);
            bmp.SetPixel(5, 6, cText); bmp.SetPixel(6, 6, cText);
            bmp.SetPixel(5, 7, cText); bmp.SetPixel(6, 7, cText);
            bmp.SetPixel(5, 8, cText); bmp.SetPixel(6, 8, cText);
            bmp.SetPixel(5, 9, cText); bmp.SetPixel(6, 9, cText);

            // 縦棒（右）
            bmp.SetPixel(9, 5, cText); bmp.SetPixel(10, 5, cText);
            bmp.SetPixel(9, 6, cText); bmp.SetPixel(10, 6, cText);
            bmp.SetPixel(9, 7, cText); bmp.SetPixel(10, 7, cText);
            bmp.SetPixel(9, 8, cText); bmp.SetPixel(10, 8, cText);
            bmp.SetPixel(9, 9, cText); bmp.SetPixel(10, 9, cText);

            // 底部横棒
            bmp.SetPixel(6, 10, cText);
            bmp.SetPixel(7, 10, cText);
            bmp.SetPixel(8, 10, cText);
            bmp.SetPixel(9, 10, cText);

            IntPtr hIcon = bmp.GetHicon();
            Icon createdIcon = (Icon)Icon.FromHandle(hIcon).Clone();
            DestroyIcon(hIcon);

            return createdIcon;
        }
    }
}
