<template>
    <div id="videoDiv" v-if="!videoNotFound">
        <VideoPlayer :aspect_ratio="ratio" :video_url="urls.video_url" :thumb_url="urls.thumb_url" />
        <div class="video-information">
            <div class="video-title">
                <h3>{{ metadata.title }}</h3>
            </div>
            <v-no-ssr>
                <v-tooltip :text="date" location="start">
                    <template v-slot:activator="{ props }">
                        <div v-bind="props" class="video-description">
                            Uploaded by {{ metadata.username }} {{ dateMain }}
                        </div>
                    </template>
                </v-tooltip>
            </v-no-ssr>
        </div>
    </div>
    <div style="margin: auto" v-else-if="videoNotFound">
        Video not found.
    </div>
</template>


<script setup lang="ts">
import VideoPlayer from '~/components/VideoPlayer.client.vue'
const loginStatus = defineModel<boolean>("loginStatus", { required: true });
const snackbarInformUser = defineModel<boolean>("snackbarInformUser", { required: true });
const snackbarInformUserText = defineModel<string>("snackbarInformUserText", { required: true });
const config = useRuntimeConfig();
const route = useRoute();
const urls = ref();
const videoNotFound = ref(false);
//default 4:3
const width = ref('965px')
const ratio = ref('4/3')
const date = ref('today')
const dateMain = ref('today')

function setResolution(resolutionDivision: number) {
    // 16:9
    if (resolutionDivision < 0.75) {
        width.value = '1350px';
        ratio.value = '16/9';
    }
    // 21:9
    if (resolutionDivision < 0.42) {
        width.value = '2580px';
        ratio.value = '21/9';
    }
}

async function getSEOMetadata(api_endpoint: string, id: string) {
    const api_seodata = api_endpoint + "/video/" + id;
    const { data, error } = await useFetch<SEOMetadata>(api_seodata, {
        method: "GET"
    });

    if (error.value != null) {
        console.log(error)
    }

    return { username: data.value?.username, title: data.value?.video.name, user: data.value?.userId, date: new Date(data.value!.video.date) }
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

function setDateStrings(metadate: Date) {
    date.value = metadate.toLocaleDateString();
    let diffDate = new Date(Math.abs(Date.now() - metadata.date.getTime()));
    let dateStringBuilder = "today";

    let oneDayInMilliseconds = 24 * 60 * 60 * 1000;
    let oneWeekInMilliseconds = oneDayInMilliseconds * 7;
    let oneMonthInMilliseconds = oneWeekInMilliseconds * 4;

    let yearsAgo = Math.floor(diffDate.getUTCFullYear() - 1970);
    let monthsAgo = Math.floor(diffDate.getTime() / oneMonthInMilliseconds);
    let weeksAgo = Math.floor(diffDate.getTime() / oneWeekInMilliseconds);
    let daysAgo = Math.floor(diffDate.getTime() / oneDayInMilliseconds);

    if (daysAgo >= 1) {
        dateStringBuilder = daysAgo.toString() + " day";
    }
    if (daysAgo >= 2) {
        dateStringBuilder = daysAgo.toString() + " days";
    }
    if (weeksAgo >= 1) {
        dateStringBuilder = weeksAgo.toString() + " week";
    }
    if (weeksAgo >= 2) {
        dateStringBuilder = weeksAgo.toString() + " weeks";
    }
    if (monthsAgo >= 1) {
        dateStringBuilder = monthsAgo.toString() + " month";
    }
    if (monthsAgo >= 2) {
        dateStringBuilder = monthsAgo.toString() + " months";
    }
    if (yearsAgo >= 1) {
        dateStringBuilder = yearsAgo.toString() + " year";
    }
    if (yearsAgo >= 2) {
        dateStringBuilder = yearsAgo.toString() + " years";
    }
    if (dateStringBuilder != "today")
        dateMain.value = dateStringBuilder + " ago"
    else
        dateMain.value = "today"
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

await nextTick();

var metadata = await getSEOMetadata(config.public.apiUrl, String(route.params.id));
if (metadata.title != undefined && metadata.username != undefined && metadata.user != undefined) {
    urls.value = getUrls(config.public.baseUrl, metadata.user, String(route.params.id));
    setSEOMeta(metadata.title, metadata.username, urls.value.video_url, urls.value.thumb_url);
}

onBeforeMount(async () => {
    if (import.meta.client && metadata.date != undefined) {
        const resolutionDivision = await getVideoDimensionsOf(urls.value.video_url);
        setResolution(resolutionDivision);
        setDateStrings(metadata.date);
    }
})


</script>

<style lang="scss" scoped>
.video-information {
    color: white;
    width: 100%;
}

.video-description {
    font-size: .9em;
}

#videoDiv {
    max-width: v-bind('width');
    width: 100%;
    margin: auto;
}
</style>