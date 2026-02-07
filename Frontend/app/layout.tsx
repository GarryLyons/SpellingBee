import type { Metadata } from "next";
import { Baloo_2, Nunito } from "next/font/google";
import "./globals.css";

const headingFont = Baloo_2({
  subsets: ["latin"],
  variable: "--font-heading",
  weight: ["600", "700", "800"]
});

const bodyFont = Nunito({
  subsets: ["latin"],
  variable: "--font-body",
  weight: ["500", "600", "700"]
});

export const metadata: Metadata = {
  title: "Spelling Bee Practice",
  description: "Adult-led reading and pronunciation practice with adaptive repetition and phonics support."
};

export default function RootLayout({
  children
}: Readonly<{
  children: React.ReactNode;
}>) {
  return (
    <html lang="en">
      <body className={`${headingFont.variable} ${bodyFont.variable}`}>{children}</body>
    </html>
  );
}
