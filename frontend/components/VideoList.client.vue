<template>
    <div class="center">
        <div style="
              width: 100%;
              display: flex;
              flex-wrap: wrap;
              justify-content: space-around;
              align-items: start;
              align-content: start;
              align-self: baseline;
            ">
            <div style="margin: 8px; flex-basis: 25rem" v-for="(video, i) in videos" :key="i">
                <VideoListItem @delete-video="deleteVideo" :video="video" />
            </div>
        </div>
    </div>
</template>

<script lang="ts" setup>
import VideoListItem from '~/components/VideoListItem.client.vue';
import { VideoHandling } from './VideoHandling';
import type { Video } from '~/utils/schemas';
const config = useRuntimeConfig();
const snackbarInformUser = defineModel<boolean>("snackbarInformUser", { required: true });
const snackbarInformUserText = defineModel<string>("snackbarInformUserText", { required: true });
const videos = defineModel<Video[]>("videos", { required: true });

let videoCount = 24;
let videoMax = 0;
let videoOffset = 0;

onBeforeMount(async () => {
    // Weird workaround... before a request is executed correctly.
    await nextTick()
    await getVideos();
    endlessScroll();
})

function endlessScroll() {
    var reachedMax = false;
    var loading = false;
    window.onscroll = async () => {
        let threshold =
            document.documentElement.scrollTop + window.innerHeight >=
            document.documentElement.offsetHeight * 0.9;
        if (threshold && videoCount < videoMax && !reachedMax && !loading) {
            videoOffset += videoCount;
            videoCount = 12;
            loading = true;
            await getVideos();
            loading = false;
        }
        if (videoCount + videoOffset > videoMax) {
            if (!reachedMax) {
                snackbarInformUserText.value = "Reached the end, all videos loaded."
                snackbarInformUser.value = true;
            }
            reachedMax = true;
        }
    }
}

async function getVideos() {
    var videorequest = await VideoHandling.fetchVideos(config.public.apiUrl, videoCount, videoOffset);
    if (videorequest.success) {
        videorequest.videos.forEach((video) => {
            videos.value.push(video);
        });
        videoMax = videorequest.max;
    }
}

function deleteVideo(id: string) {
    videos.value.forEach((video) => {
        if (video.id === id) {
            videos.value.splice(videos.value.indexOf(video), 1);
        }
    })
}

</script>

<style lang="scss" scoped>
.center {
    height: 100%;
    display: flex;
    justify-content: center;
    align-items: center;
    width: 100%;
}
</style>