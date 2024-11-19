package com.microsoft.maui;

import java.nio.charset.StandardCharsets;

public class PlatformUtils {
    public static String getGlyphHex(String glyph) {
        if (glyph == null) {
            return null;
        }

        byte[] ba = glyph.getBytes(StandardCharsets.UTF_16);
        StringBuilder str = new StringBuilder();
        for(int i = 2 /* Skip (BOM) */; i < ba.length; i++)
            str.append(String.format("%02x", ba[i]));
        return str.toString();
    }
}