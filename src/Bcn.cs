// Bcn.cs — декодер BC1 / BC3 (DXT1 / DXT5) -> RGBA. Heat Soundtrack: фоны окна из текстур игры (в Heat они BC1 / BC3, в Unbound были BC7 — Bc7.cs).
using System;

static class Bcn
{
    // bc3 = false — BC1 (8 байт на блок 4x4), true — BC3 (16 байт: 8 альфа + 8 цвет как BC1)
    public static byte[] Decode(byte[] data, int w, int h, bool bc3)
    {
        var rgba = new byte[w * h * 4];
        int bw = (w + 3) / 4, bh = (h + 3) / 4, bs = bc3 ? 16 : 8;
        var col = new byte[16]; var a = new byte[8]; var ai = new int[16];
        for (int by = 0; by < bh; by++)
            for (int bx = 0; bx < bw; bx++)
            {
                int o = (by * bw + bx) * bs;
                if (o + bs > data.Length) return rgba;
                if (bc3)
                {
                    a[0] = data[o]; a[1] = data[o + 1];
                    if (a[0] > a[1]) for (int i = 1; i < 7; i++) a[i + 1] = (byte)(((7 - i) * a[0] + i * a[1]) / 7);
                    else { for (int i = 1; i < 5; i++) a[i + 1] = (byte)(((5 - i) * a[0] + i * a[1]) / 5); a[6] = 0; a[7] = 255; }
                    ulong bits = 0; for (int i = 0; i < 6; i++) bits |= (ulong)data[o + 2 + i] << (8 * i);
                    for (int i = 0; i < 16; i++) ai[i] = (int)((bits >> (3 * i)) & 7);
                    o += 8;
                }
                int c0 = data[o] | data[o + 1] << 8, c1 = data[o + 2] | data[o + 3] << 8;
                Rgb565(c0, col, 0); Rgb565(c1, col, 4);
                bool four = bc3 || c0 > c1;
                for (int k = 0; k < 3; k++)
                {
                    if (four) { col[8 + k] = (byte)((2 * col[k] + col[4 + k]) / 3); col[12 + k] = (byte)((col[k] + 2 * col[4 + k]) / 3); }
                    else { col[8 + k] = (byte)((col[k] + col[4 + k]) / 2); col[12 + k] = 0; }
                }
                col[3] = col[7] = col[11] = 255; col[15] = (byte)(four ? 255 : 0);
                uint idx = (uint)(data[o + 4] | data[o + 5] << 8 | data[o + 6] << 16 | data[o + 7] << 24);
                for (int py = 0; py < 4; py++)
                    for (int px = 0; px < 4; px++)
                    {
                        int x = bx * 4 + px, y = by * 4 + py;
                        if (x >= w || y >= h) continue;
                        int p = py * 4 + px, ci = (int)((idx >> (2 * p)) & 3) * 4, d = (y * w + x) * 4;
                        rgba[d] = col[ci]; rgba[d + 1] = col[ci + 1]; rgba[d + 2] = col[ci + 2];
                        rgba[d + 3] = bc3 ? a[ai[p]] : col[ci + 3];
                    }
            }
        return rgba;
    }

    static void Rgb565(int c, byte[] o, int i)
    {
        int r = (c >> 11) & 31, g = (c >> 5) & 63, b = c & 31;
        o[i] = (byte)(r << 3 | r >> 2); o[i + 1] = (byte)(g << 2 | g >> 4); o[i + 2] = (byte)(b << 3 | b >> 2);
    }
}
