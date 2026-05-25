"use client";

import { EntityPage } from "@/cmms/EntityPage";
import { rootCauseConfig } from "@/cmms/entity-config";

export default function RootCauses() {
  return <EntityPage config={rootCauseConfig} />;
}
