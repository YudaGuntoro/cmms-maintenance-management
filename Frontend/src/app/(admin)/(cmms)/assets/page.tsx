"use client";

import { EntityPage } from "@/cmms/EntityPage";
import { assetConfig } from "@/cmms/entity-config";

export default function Assets() {
  return <EntityPage config={assetConfig} />;
}
