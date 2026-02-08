import { NextResponse } from "next/server";

const BACKEND_URL = process.env.BACKEND_API_URL ?? "http://localhost:5064";

export async function GET() {
  try {
    const response = await fetch(`${BACKEND_URL}/api/words`, {
      cache: "no-store",
    });

    if (!response.ok) {
        const errorData = await response.json().catch(() => null);
        return NextResponse.json(errorData || { message: "Backend request failed" }, { status: response.status });
    }

    const data = await response.json();
    return NextResponse.json(data);
  } catch (error) {
    console.error("BFF Error:", error);
    return NextResponse.json({ message: "Internal Server Error" }, { status: 500 });
  }
}
