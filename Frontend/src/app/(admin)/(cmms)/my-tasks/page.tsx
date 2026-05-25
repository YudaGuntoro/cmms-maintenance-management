import MyTasksPage from "@/cmms/MyTasksPage";
import { Metadata } from "next";

export const metadata: Metadata = {
  title: "My Task | Computerized Maintenance Management System (CMMS)",
  description: "Assigned maintenance work orders",
};

export default function MyTasks() {
  return <MyTasksPage />;
}
