"use client";

type IconProps = {
  name: "activity" | "alert" | "box" | "calendar" | "camera" | "check" | "copy" | "edit" | "image" | "login" | "plus" | "refresh" | "search" | "trash" | "upload" | "wrench" | "x";
  className?: string;
};

const iconPaths: Record<IconProps["name"], string[]> = {
  activity: ["M22 12h-4l-3 7L9 5l-3 7H2"],
  alert: ["M12 9v4", "M12 17h.01", "M10.29 3.86 1.82 18a2 2 0 0 0 1.71 3h16.94a2 2 0 0 0 1.71-3L13.71 3.86a2 2 0 0 0-3.42 0Z"],
  box: ["m21 8-9-5-9 5 9 5 9-5Z", "M3 8v8l9 5 9-5V8", "M12 13v8"],
  calendar: ["M8 2v4", "M16 2v4", "M3 10h18", "M5 4h14a2 2 0 0 1 2 2v14a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2V6a2 2 0 0 1 2-2Z"],
  camera: ["M14.5 4 16 7h3a2 2 0 0 1 2 2v9a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2V9a2 2 0 0 1 2-2h3l1.5-3h5Z", "M12 17a4 4 0 1 0 0-8 4 4 0 0 0 0 8Z"],
  check: ["M20 6 9 17l-5-5"],
  copy: ["M8 8h10a2 2 0 0 1 2 2v10a2 2 0 0 1-2 2H8a2 2 0 0 1-2-2V10a2 2 0 0 1 2-2Z", "M16 8V4a2 2 0 0 0-2-2H4a2 2 0 0 0-2 2v10a2 2 0 0 0 2 2h2"],
  edit: ["M12 20h9", "M16.5 3.5a2.12 2.12 0 0 1 3 3L7 19l-4 1 1-4 12.5-12.5Z"],
  image: ["M19 3H5a2 2 0 0 0-2 2v14a2 2 0 0 0 2 2h14a2 2 0 0 0 2-2V5a2 2 0 0 0-2-2Z", "m21 15-4-4a2 2 0 0 0-2.8 0L6 19", "M8.5 9.5h.01"],
  login: ["M15 3h4a2 2 0 0 1 2 2v14a2 2 0 0 1-2 2h-4", "m10 17 5-5-5-5", "M15 12H3"],
  plus: ["M12 5v14", "M5 12h14"],
  refresh: ["M21 12a9 9 0 0 1-15.5 6.2L3 16", "M3 21v-5h5", "M3 12A9 9 0 0 1 18.5 5.8L21 8", "M21 3v5h-5"],
  search: ["m21 21-4.3-4.3", "M11 19a8 8 0 1 1 0-16 8 8 0 0 1 0 16Z"],
  trash: ["M3 6h18", "M8 6V4h8v2", "M19 6l-1 14H6L5 6", "M10 11v6", "M14 11v6"],
  upload: ["M12 3v12", "m7 8 5-5 5 5", "M5 21h14"],
  wrench: ["M14.7 6.3a4 4 0 0 0-5.4 5.4L3 18l3 3 6.3-6.3a4 4 0 0 0 5.4-5.4L15 12l-3-3 2.7-2.7Z"],
  x: ["M18 6 6 18", "M6 6l12 12"],
};

export function Icon({ name, className = "h-5 w-5" }: IconProps) {
  return (
    <svg aria-hidden="true" className={className} fill="none" stroke="currentColor" strokeLinecap="round" strokeLinejoin="round" strokeWidth={1.8} viewBox="0 0 24 24">
      {iconPaths[name].map((path) => (
        <path d={path} key={path} />
      ))}
    </svg>
  );
}
