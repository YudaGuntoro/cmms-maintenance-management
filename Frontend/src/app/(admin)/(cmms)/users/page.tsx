"use client";

import { EntityPage } from "@/cmms/EntityPage";
import { userConfig } from "@/cmms/entity-config";

export default function Users() {
  return <EntityPage config={userConfig} />;
}
