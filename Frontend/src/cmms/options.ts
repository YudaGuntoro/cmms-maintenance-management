import {
  appUserRoles,
  appUserStatuses,
  assetCriticalityLevels,
  assetStatuses,
  contractorDocumentStatuses,
  contractorDocumentTypes,
  contractorWorkStatuses,
  downtimeCategories,
  frequencyTypes,
  maintenanceTypes,
  problemReportCategories,
  problemReportStatuses,
  technicianSkillTypes,
  technicianStatuses,
  workOrderPriorities,
  workOrderStatuses,
} from "./types";
import { statusText } from "./format";

export type SelectOption = {
  label: string;
  value: string;
};

export function enumOptions(values: readonly string[]): SelectOption[] {
  return values.map((value) => ({
    label: statusText(value),
    value,
  }));
}

export const options = {
  appUserRole: enumOptions(appUserRoles),
  appUserStatus: enumOptions(appUserStatuses),
  assetCriticality: enumOptions(assetCriticalityLevels),
  assetStatus: enumOptions(assetStatuses),
  boolean: [
    { label: "Yes", value: "true" },
    { label: "No", value: "false" },
  ],
  contractorDocumentStatus: enumOptions(contractorDocumentStatuses),
  contractorDocumentType: enumOptions(contractorDocumentTypes),
  contractorWorkStatus: enumOptions(contractorWorkStatuses),
  downtimeCategory: enumOptions(downtimeCategories),
  frequencyType: enumOptions(frequencyTypes),
  maintenanceType: enumOptions(maintenanceTypes),
  problemReportCategory: enumOptions(problemReportCategories),
  problemReportStatus: enumOptions(problemReportStatuses),
  technicianSkill: enumOptions(technicianSkillTypes),
  technicianStatus: enumOptions(technicianStatuses),
  workOrderPriority: enumOptions(workOrderPriorities),
  workOrderStatus: enumOptions(workOrderStatuses),
};
