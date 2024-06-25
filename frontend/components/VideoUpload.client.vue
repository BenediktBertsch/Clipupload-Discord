<template>
    <v-card rounded="lg" elevation="16" style="position: fixed; width: 350px; bottom: -10px; right: 10px;">
        <v-card-actions>
            <v-card-title>Uploads</v-card-title>
            <v-spacer></v-spacer>
            <v-btn :icon="show ? 'mdi-chevron-up' : 'mdi-chevron-down'" @click="show = !show"></v-btn>
        </v-card-actions>

        <v-expand-transition>
            <div v-show="show">
                <v-divider></v-divider>
                <v-row no-gutters style="height: 75px" v-for="(video, i) in uploadVideos" :key="i">
                    <v-col class="center" :cols="3">
                        <img style="height: 30px" :src="video.previewUrl" />
                    </v-col>
                    <v-col :cols="8" class="center">
                        <span>
                            {{ videoTitle(video.video.name) }}
                        </span>
                    </v-col>
                    <v-col v-if="video.uploading" class="center" style="padding: unset" :cols="1"
                        @mouseenter="video.hover = true" @mouseleave="video.hover = false">
                        <div v-if="!video.hover">
                            <v-progress-circular :model-value="video.uploadProgress" size="x-small"
                                :width="2"></v-progress-circular>
                        </div>
                        <div v-else>
                            <v-btn class="v-progress-circular--size-x-small" @click="abort(i)" icon size="20">
                                <v-icon size="16">mdi-close</v-icon>
                            </v-btn>
                        </div>
                    </v-col>
                    <v-col v-else-if="video.finished" class="center" style="padding: unset" :cols="1">
                        <v-icon small>
                            mdi-check
                        </v-icon>
                    </v-col>
                    <v-col :cols="1" v-else class="center" style="padding: unset" @mouseenter="video.hover = true"
                        @mouseleave="video.hover = false">
                        <div v-if="!video.hover">
                            <v-progress-circular :model-value="video.uploadProgress" size="x-small"
                                :width="2"></v-progress-circular>
                        </div>
                        <div v-else>
                            <v-btn class="v-progress-circular--size-x-small" @click="abort(i)" icon size="20">
                                <v-icon size="16">mdi-close</v-icon>
                            </v-btn>
                        </div>
                    </v-col>
                </v-row>
            </div>
        </v-expand-transition>
    </v-card>
</template>

<script lang="ts" setup>
import type { Video, VideoUpload, VideoUploadResultFailed, VideoUploadResultSuccess } from '~/utils/schemas';

const uploadVideos = defineModel<VideoUpload[]>("uploadVideos", { required: true });
const config = useRuntimeConfig();
const emit = defineEmits<{
    (e: 'addVideo', video: Video): void
}>()
const snackBarUpload = ref(true);
const show = ref(true);
const uploadingIndex = ref(0), delIndex = ref(0);
var request = new XMLHttpRequest();

onMounted(async () => {
    snackBarUpload.value = true;
    await uploadingVideos(uploadVideos.value!);
})

onUnmounted(() => {
    console.log("Upload unmounted!")
    uploadVideos.value = [];
})

function update_progress(e: ProgressEvent) {
    uploadVideos.value![uploadingIndex.value - delIndex.value].uploadProgress = Math.floor(e.loaded / (uploadVideos.value![uploadingIndex.value - delIndex.value].video.size / 100));
}

async function uploadVideo(file: VideoUpload) {
    return await new Promise<VideoUploadResultSuccess | VideoUploadResultFailed>((resolve) => {
        let data = new FormData();
        data.append("file", file.video)
        request = new XMLHttpRequest();
        request.upload.addEventListener("progress", update_progress, false);
        request.addEventListener("abort", function () {
            delIndex.value++;
            resolve({ success: false, error: "Upload aborted!" });
        });
        request.addEventListener("error", function () {
            delIndex.value++;
            resolve({ success: false, error: "Upload failed!" });
        });
        request.addEventListener("readystatechange", function () {
            if (request.readyState === XMLHttpRequest.DONE &&
                request.status === 200 && !file.finished
            ) {
                var ans = JSON.parse(request.responseText);
                if (ans.success) {
                    file.uploading = false;
                    file.finished = true;
                }
                resolve(ans);
            }
            if (request.readyState === XMLHttpRequest.DONE &&
                request.status === 500 && !file.finished
            ) {
                var ans = JSON.parse(request.responseText);
                file.uploading = false;
                file.finished = true;
                resolve({ success: false, error: ans.detail })
            }
        });
        request.open("POST", `${config.public.api_url}upload`);
        request.setRequestHeader("Authorization",
            "Bearer " + localStorage.getItem("token"));
        request.send(data);
    });
}

async function uploadingVideos(files: VideoUpload[]) {
    delIndex.value = 0;
    uploadingIndex.value = 0;
    for (let i = 0; i < files.length + delIndex.value; i++) {
        const currentVideo = files[i - delIndex.value];
        if (!currentVideo.finished) {
            uploadingIndex.value = i;
            files[i - delIndex.value].uploading = true;
            var newVideo = await uploadVideo(currentVideo);
            if (uploadSuccess(newVideo))
                emit("addVideo", newVideo.video);
        }
    }
    await new Promise(resolve => setTimeout(resolve, 2000));
    uploadVideos.value = [];
}

function uploadSuccess(result: VideoUploadResultSuccess | VideoUploadResultFailed): result is VideoUploadResultSuccess {
    return (<VideoUploadResultSuccess>result).video !== undefined;
}

function videoTitle(videoname: string) {
    if (videoname.length > 25) {
        var name = videoname.slice(0, 25);
        if (name[name.length] === " ") {
            return name + "...";
        } else {
            return name + " ...";
        }
    } else {
        return videoname;
    }
}

function abort(index: number) {
    uploadVideos.value!.splice(index - delIndex.value, 1);
    if (uploadVideos.value![index].uploading) {
        request.abort();
    }
}
</script>

<style lang="scss" scoped>
.switch-center {
    display: flex;
    justify-content: center;
}

.center {
    height: auto;
    display: flex;
    justify-content: center;
    align-items: center;
    width: 100%;
}

.notCentered {
    height: auto;
    display: flex;
    align-items: center;
    width: 100%;
}

.end {
    height: auto;
    display: flex;
    justify-content: flex-end;
    align-items: center;
    width: 100%;
}

.start {
    height: auto;
    display: flex;
    justify-content: flex-start;
    align-items: center;
    width: 100%;
}
</style>
