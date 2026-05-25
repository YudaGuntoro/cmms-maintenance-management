import React from "react";
import {
  AlertIcon,
  BoxCubeIcon,
  BoltIcon,
  CheckCircleIcon,
  DocsIcon,
  GridIcon,
  GroupIcon,
  PlugInIcon,
  UserCircleIcon,
} from "../icons/index";

export type NavSubItem = {
  name: string;
  path: string;
  pro?: boolean;
  new?: boolean;
};

export type NavItem = {
  name: string;
  icon: React.ReactNode;
  path?: string;
  subItems?: NavSubItem[];
};

export const navItems: NavItem[] = [
  {
    icon: <GridIcon />,
    name: "Overview",
    path: "/",
  },
  {
    icon: <CheckCircleIcon />,
    name: "MyTask",
    path: "/my-tasks",
  },
  {
    icon: <AlertIcon />,
    name: "Report Problem",
    path: "/report-problem",
  },
  {
    icon: <BoxCubeIcon />,
    name: "MasterData",
    subItems: [
      { name: "Assets", path: "/assets" },
      { name: "Technicians", path: "/technicians" },
      { name: "Spareparts", path: "/spareparts" },
      { name: "CMMS Masters", path: "/master-data" },
      { name: "Failure Codes", path: "/failure-codes" },
      { name: "Root Causes", path: "/root-causes" },
    ],
  },
  {
    icon: <BoltIcon />,
    name: "Operation",
    subItems: [
      { name: "Work Orders", path: "/work-orders" },
      { name: "Maintenance History", path: "/maintenance-history" },
      { name: "Contractor Monitoring", path: "/contractor-monitoring" },
      { name: "Preventive", path: "/preventive-schedules" },
      { name: "Downtime Logs", path: "/downtime-logs" },
    ],
  },
  {
    icon: <GroupIcon />,
    name: "Technician Performance",
    path: "/technician-performance",
  },
  {
    icon: <DocsIcon />,
    name: "Integration",
    path: "/integration",
  },
  {
    icon: <UserCircleIcon />,
    name: "Users",
    path: "/users",
  },
  {
    icon: <PlugInIcon />,
    name: "Settings",
    path: "/settings",
  },
];
