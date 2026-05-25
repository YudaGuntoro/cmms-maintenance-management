import SignInPage from "@/cmms/SignInPage";
import { Metadata } from "next";

export const metadata: Metadata = {
  title: "Sign In | Computerized Maintenance Management System (CMMS)",
  description: "Computerized Maintenance Management System (CMMS) sign in",
};

export default function SignIn() {
  return <SignInPage />;
}
