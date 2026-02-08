"use client";

import { Amplify } from "aws-amplify";
import { authConfig } from "@/lib/auth-config";

Amplify.configure(authConfig, { ssr: true });

export default function ConfigureAmplify() {
  return null;
}
