import { NextRequest, NextResponse } from "next/server";

const BACKEND_URL = process.env.BACKEND_API_URL ?? "http://localhost:5064";

export async function POST(
  request: NextRequest,
  { params }: { params: Promise<{ sessionId: string }> }
) {
  try {
    // In Next.js 15 params are async, likely this is Next 14 but good practice to await or treat as safe objects if types allow. 
    // In Next 14 App Router params is not a promise, but in 15 it is. The context says Next 14.
    // However, basic usage:
    const { sessionId } = await params; 
    const body = await request.json();

    const response = await fetch(`${BACKEND_URL}/api/practice-sessions/${sessionId}/attempts`, {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
      },
      body: JSON.stringify(body),
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
