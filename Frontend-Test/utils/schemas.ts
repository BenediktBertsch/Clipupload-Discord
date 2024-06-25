export interface SEOMetadata {
  success: boolean;
  name: string;
  username: string;
}

export interface LoginRequest {
  access_token: string;
  expires_in: string;
  refresh_token: string;
}

export interface Video {
  date: Date,
  id: string,
  hash: string,
  name: string,
  user: number
}

export interface VideosRequest {
  max: number,
  success: boolean,
  videoCount: number,
  videos: Video[],
}

export interface VideoUploadResultSuccess {
  success: boolean,
  video: Video,
}

export interface VideoUploadResultFailed {
  success: boolean,
  error: string,
}

export interface VideoUpload {
  video: File,
  uploading: boolean,
  finished: boolean,
  hover: boolean,
  uploadProgress: number,
  previewUrl: string
}