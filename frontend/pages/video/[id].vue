<template>
    <div :style="{'max-width': width, 'width': '100%', 'margin': 'auto'}" v-if="!videoLoading && !videoNotFound">
        <VideoPlayer :video_url="urls.video_url" :thumb_url="urls.thumb_url" />
    </div>
    <div style="margin: auto" v-else-if="!videoLoading && videoNotFound">
        Video not found.
    </div>
</template>

<script setup lang="ts">
import VideoPlayer from '~/components/VideoPlayer.client.vue'
const loginStatus = defineModel<boolean>("loginStatus", { required: true });
const snackbarInformUser = defineModel<boolean>("snackbarInformUser", { required: true });
const snackbarInformUserText = defineModel<string>("snackbarInformUserText", { required: true });
const videoLoading = ref(true);
const config = useRuntimeConfig();
const route = useRoute();
const urls = ref();
const videoNotFound = ref(false);
const width = ref('1540px')

onBeforeMount(async () => {
    // Weird workaround... before a request is executed correctly.
    await nextTick()
    const param_id = String(route.params.id);
    var metadata = await getSEOMetadata(config.public.apiUrl, param_id);
    videoLoading.value = false;
    if (metadata.title != undefined && metadata.username != undefined && metadata.user != undefined) {
        urls.value = getUrls(config.public.baseUrl, metadata.user, param_id);
        const resolutionDivision = await getVideoDimensionsOf(urls.value.video_url);
        setResolution(resolutionDivision);
        console.log(width.value)
        setSEOMeta(metadata.title, metadata.username, urls.value.video_url, urls.value.thumb_url);
    }
    else {
        videoNotFound.value = true;
    }
})

function setResolution(resolutionDivision: number) {
    // 16:9
    if(resolutionDivision < 0.75) {
        width.value = '1900px';
    }
    // 21:9
    if(resolutionDivision < 0.42) {
        width.value = '2960px';
    }
}

async function getSEOMetadata(api_endpoint: string, id: string) {
    const api_seodata = api_endpoint + "/video/" + id
    const data = await $fetch<SEOMetadata>(api_seodata, {
        method: "GET"
    })
    return { username: data.username, title: data.video.name, user: data.userId }
}

function getUrls(base_url: string, user: string, id: string) {
    return { video_url: base_url + "/files/" + user + "/" + id + '.mp4', thumb_url: base_url + "/files/" + user + "/" + id + '.avif' }
}

function getVideoDimensionsOf(url: string): Promise<number> {
    return new Promise(resolve => {
        const video = document.createElement('video');
        video.addEventListener("loadedmetadata", function () {
            const height = this.videoHeight;
            const width = this.videoWidth;
            resolve(height / width);
        }, false);
        video.src = url;
    });
}

function setSEOMeta(title: string, username: string, video_url: string, thumb_url: string,) {
    useHead({
        title,
        meta: [
            { property: 'title', content: title },
            { property: 'og:type', content: 'video.other' },
            { property: 'og:video:type', content: 'video/mp4' },
            { property: 'og:url', content: 'Posted by: ' + username },
            { property: 'og:video', content: video_url },
            { property: 'og:video:width', content: '640' },
            { property: 'og:video:height', content: '426' },
            { property: 'og:video:type', content: 'application/x-shockwave-flash' },
            { property: 'og:video:url', content: video_url },
            { property: 'og:video:secure_url', content: video_url },
            { property: 'og:title', content: title },
            { property: 'og:image', content: thumb_url },
            { property: 'og:site_name', content: 'Posted by: ' + username },
        ]
    })
}
</script>