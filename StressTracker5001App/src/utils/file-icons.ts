import type { LucideIcon } from "lucide-react";
import {
  ArchiveIcon,
  CodeIcon,
  FileIcon,
  FileImageIcon,
  FileJsonIcon,
  FileSpreadsheetIcon,
  FileStackIcon,
  FileTextIcon,
  MusicIcon,
  VideoIcon,
} from "lucide-react";

type FileExtension = string;

const fileIconMap: Record<FileExtension, LucideIcon> = {
  // Documents
  ".pdf": FileTextIcon,
  ".doc": FileStackIcon,
  ".docx": FileStackIcon,
  ".txt": FileTextIcon,
  ".rtf": FileTextIcon,

  // Spreadsheets
  ".xls": FileSpreadsheetIcon,
  ".xlsx": FileSpreadsheetIcon,
  ".csv": FileSpreadsheetIcon,
  ".ods": FileSpreadsheetIcon,

  // Presentations
  ".ppt": FileStackIcon,
  ".pptx": FileStackIcon,
  ".odp": FileStackIcon,

  // Images
  ".jpg": FileImageIcon,
  ".jpeg": FileImageIcon,
  ".png": FileImageIcon,
  ".gif": FileImageIcon,
  ".bmp": FileImageIcon,
  ".svg": FileImageIcon,
  ".webp": FileImageIcon,
  ".ico": FileImageIcon,

  // Archives
  ".zip": ArchiveIcon,
  ".rar": ArchiveIcon,
  ".7z": ArchiveIcon,
  ".tar": ArchiveIcon,
  ".gz": ArchiveIcon,

  // Code
  ".js": CodeIcon,
  ".ts": CodeIcon,
  ".jsx": CodeIcon,
  ".tsx": CodeIcon,
  ".json": FileJsonIcon,
  ".xml": CodeIcon,
  ".html": CodeIcon,
  ".css": CodeIcon,
  ".py": CodeIcon,
  ".java": CodeIcon,
  ".cpp": CodeIcon,
  ".c": CodeIcon,
  ".cs": CodeIcon,

  // Media
  ".mp3": MusicIcon,
  ".wav": MusicIcon,
  ".flac": MusicIcon,
  ".aac": MusicIcon,
  ".m4a": MusicIcon,
  ".mp4": VideoIcon,
  ".avi": VideoIcon,
  ".mov": VideoIcon,
  ".mkv": VideoIcon,
  ".webm": VideoIcon,
  ".flv": VideoIcon,
};

export function getFileIcon(fileName: string): LucideIcon {
  const extension = fileName.slice(fileName.lastIndexOf(".")).toLowerCase();
  return fileIconMap[extension] ?? FileIcon;
}

export function getFileExtension(fileName: string): string {
  return fileName.slice(fileName.lastIndexOf(".")).toLowerCase();
}

export function isImageFile(fileName: string): boolean {
  const imageExtensions = [".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp"];
  const extension = fileName.slice(fileName.lastIndexOf(".")).toLowerCase();
  return imageExtensions.includes(extension);
}

export function formatFileSize(bytes: number): string {
  if (bytes === 0) return "0 Bytes";

  const k = 1024;
  const sizes = ["Bytes", "KB", "MB", "GB"];
  const i = Math.floor(Math.log(bytes) / Math.log(k));

  return Math.round((bytes / Math.pow(k, i)) * 100) / 100 + " " + sizes[i];
}
