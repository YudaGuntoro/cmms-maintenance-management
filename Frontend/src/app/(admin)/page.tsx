import type { Metadata } from "next";
import DashboardPage from "@/cmms/DashboardPage";

export const metadata: Metadata = {
  title: "Maintenance Dashboard",
  description: "Maintenance management dashboard",
};

export default function Ecommerce() {
  return <DashboardPage />;
}
