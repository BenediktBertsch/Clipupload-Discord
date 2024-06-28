<template>
    <VideoPlayer v-if="!videoLoading && !VIDEO_NOT_FOUND"
        style="height: 75%;" :video_url="urls.video_url" :thumb_url="urls.thumb_url" />
    <div v-else-if="!videoLoading && VIDEO_NOT_FOUND">
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
let VIDEO_NOT_FOUND = ref(false);

onBeforeMount(async () => {
    // Weird workaround... before a request is executed correctly.
    await nextTick()
    const param_id = String(route.params.id);
    var metadata = await getSEOMetadata(config.public.apiUrl, param_id);
    videoLoading.value = false;
    if (metadata.title != undefined && metadata.username != undefined && metadata.user != undefined) {
        urls.value = getUrls(config.public.baseUrl, metadata.user, param_id);
        setSEOMeta(metadata.title, metadata.username, urls.value.video_url, urls.value.thumb_url);
    }
    else {
        VIDEO_NOT_FOUND.value = true;
    }
})

async function getSEOMetadata(api_endpoint: string, id: string) {
    const api_seodata = api_endpoint + "/video/" + id
    console.log(api_seodata)
    const { data, error } = await useFetch<SEOMetadata>(api_seodata, {
        method: "GET"
    })
    return { username: data.value?.username, title: data.value?.video.name, user: data.value?.userId }
}

function getUrls(base_url: string, user: string, id: string) {
    return { video_url: base_url + "/files/" + user + "/" + id + '.mp4', thumb_url: base_url + "/files/" + user + "/" + id + '.avif' }
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