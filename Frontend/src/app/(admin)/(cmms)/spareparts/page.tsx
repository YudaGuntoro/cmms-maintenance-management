"use client";

import { EntityPage } from "@/cmms/EntityPage";
import { sparepartConfig } from "@/cmms/entity-config";

export default function Spareparts() {
  return <EntityPage config={sparepartConfig} />;
}
