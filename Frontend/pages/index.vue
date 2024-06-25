<template>
  <div v-if="loginStatus" style="padding-top: 75px;" v-on:dragover.prevent v-on:dragenter.prevent="onDragEnter"
    v-on:dragleave.prevent="onDragLeave" v-on:drop="getFiles" id="dropzone">
    <!-- <input id="file" type="file" accept="video/mp4" @change="getFiles" multiple hidden /> -->
    <VideoList v-model:snackbar-inform-user="snackbarInformUser"
      v-model:snackbar-inform-user-text="snackbarInformUserText" v-model:videos="videos" />
    <Suspense>
      <VideoUploadClient @add-video="addVideo" v-if="uploadVideos.length > 0" v-model:upload-videos="uploadVideos" />
    </Suspense>

  </div>
  <h1 v-else>Not logged in.</h1>
</template>

<script lang="ts" setup>
import VideoList from '~/components/VideoList.client.vue';
import VideoUploadClient from '~/components/VideoUpload.client.vue';
import type { Video, VideoUpload } from '~/utils/schemas';
const loginStatus = defineModel<boolean>("loginStatus", { required: true });
const snackbarInformUser = defineModel<boolean>("snackbarInformUser", { required: true });
const snackbarInformUserText = defineModel<string>("snackbarInformUserText", { required: true });
const videos = ref([] as Video[]);

let dragCounter = 0;
let uploadVideos = ref([] as VideoUpload[]);

watch(uploadVideos, (newUploadVideos: VideoUpload[]) => {
  uploadVideos.value = newUploadVideos;
})

function onDragEnter() {
  dragCounter++;
  document.getElementById("dropzone")!.classList.add("dropzone");
}

function onDragLeave() {
  dragCounter--;
  if (dragCounter === 0) {
    document.getElementById("dropzone")!.classList.remove("dropzone");
  }
}

async function getFiles(e: DragEvent) {
  e.preventDefault();
  dragCounter = 0;
  document.getElementById("dropzone")!.classList.remove("dropzone");
  if (e.dataTransfer && e.dataTransfer.items) {
    [...e.dataTransfer.items].forEach(async (item) => {
      if (item.kind === "file" && item.type === "video/mp4") {
        const videoFile = item.getAsFile() as File;
        const videoPreview = URL.createObjectURL(
          await createVideoCover(videoFile)
        );
        uploadVideos.value.push({
          video: videoFile,
          uploading: false,
          finished: false,
          hover: false,
          uploadProgress: 0,
          previewUrl: videoPreview,
        });
      }
    });
  }
}

async function createVideoCover(file: File, seekTo = 0.0): Promise<Blob> {
  return new Promise((resolve) => {
    const videoPlayer = document.createElement("video");
    videoPlayer.setAttribute("src", URL.createObjectURL(file));
    videoPlayer.load();
    videoPlayer.addEventListener("loadedmetadata", () => {
      setTimeout(() => {
        videoPlayer.currentTime = seekTo;
      }, 200);
      videoPlayer.addEventListener("seeked", () => {
        const canvas = document.createElement("canvas");
        canvas.width = videoPlayer.videoWidth;
        canvas.height = videoPlayer.videoHeight;
        const ctx = canvas.getContext("2d");
        ctx!.drawImage(videoPlayer, 0, 0, canvas.width, canvas.height);
        ctx!.canvas.toBlob(
          (blob) => {
            resolve(blob as Blob);
          },
          "image/jpeg",
          1
        );
      });
    });
  });
}

function addVideo(video: Video) {
  videos.value.unshift(video);
}
</script>

<style lang="scss" scoped>
.dropzone {
  background-color: #94949457;
}
</style>