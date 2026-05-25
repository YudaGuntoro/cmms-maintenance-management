"use client";

import { EntityPage } from "@/cmms/EntityPage";
import { technicianConfig } from "@/cmms/entity-config";

export default function Technicians() {
  return <EntityPage config={technicianConfig} />;
}
