import { NextRequest, NextResponse } from "next/server";

const BACKEND_URL = process.env.BACKEND_API_URL ?? "http://localhost:5064";

export async function POST(
  request: NextRequest,
  { params }: { params: Promise<{ sessionId: string }> }
) {
  try {
    const { sessionId } = await params;
    
    // Check if body is needed, usually empty for this endpoint based on client code
    // But we should pass it if it exists or just empty obj
    
    const response = await fetch(`${BACKEND_URL}/api/practice-sessions/${sessionId}/model-completions`, {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
      },
      body: JSON.stringify({}),
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
