<template>
    <v-card :elevation="8" :title="VideoHandling.formatVideoName(video.name)">
        <v-tooltip top>
            <span>{{ video.name }}</span>
        </v-tooltip>
        <v-img @click="openVideoDialog()" style="width: 100%; height: auto"
            :src="`${config.public.base_url}${video.id}.avif`" />
        <v-btn @click="copyVideoURL(video.id)" color="success" tile style="width: 100%">
            Share
        </v-btn>
        <v-divider></v-divider>
        <v-btn @click="openDeleteVideoDialog()" color="error" tile style="width: 100%">
            Delete
        </v-btn>
        <v-dialog style="max-width: 50%;" v-model="dialog">
            <v-card>
                <v-card-title> {{ video.name }} </v-card-title>
                    <VideoPlayer :video_url="`${config.public.baseUrl}${video.id}.mp4`" :thumb_url="`${config.public.baseUrl}${video.id}.avif`" />
                <v-card-actions>
                    <v-spacer></v-spacer>
                    <v-btn color="error" @click="dialog = false" style="width: 100%">
                        Close
                    </v-btn>
                </v-card-actions>
            </v-card>
        </v-dialog>
        <v-dialog v-model="deleteDialog">
            <v-card>
                <v-card-title> Delete "{{ video.name }}"?</v-card-title>
                <v-card-text>
                    This operation will permanently delete this video.<br />
                    This operation cannot be undone.
                </v-card-text>
                <v-card-actions>
                    <v-spacer></v-spacer>
                    <v-btn color="error" @click="deleteDialog = false" style="width: 50%">
                        Close
                    </v-btn>
                    <v-btn color="success" :disabled="timer > 0" @click="deleteVideo(video.id)" style="width: 50%">
                        <div v-if="timer > 0">Delete ({{ timer }})</div>
                        <div v-else>Delete</div>
                    </v-btn>
                </v-card-actions>
            </v-card>
        </v-dialog>
    </v-card>
</template>

<script lang="ts" setup>
import { VideoHandling } from './VideoHandling';
import type { Video } from '~/utils/schemas';
const props = defineProps<{
    video: Video
}>()
const emit = defineEmits<{
    (e: 'deleteVideo', id: string): void
}>()
const config = useRuntimeConfig();
let dialog = ref(false);
let deleteDialog = ref(false);
let timer = ref(0);

onMounted(() => {
    setInterval(() => {
        if (deleteDialog && timer.value > 0) {
            timer.value--;
        }
    }, 1000);
})

function openVideoDialog() {
    dialog.value = true;
}

function openDeleteVideoDialog() {
    deleteDialog.value = true;
    timer.value = 5;
}

async function deleteVideo(id: string) {
    const resp = await VideoHandling.deleteVideo(config.public.apiUrl, id)
    // Snackbar
    if (resp.success) {
        emit("deleteVideo", id);
        deleteDialog.value = false;
    } else {
        
    }
}

function copyVideoURL(id: string) {
    navigator.clipboard.writeText(`${config.public.baseUrl}video/${id}`);
}

</script>