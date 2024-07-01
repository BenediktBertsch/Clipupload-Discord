<template>
    <v-card :elevation="8">
        <v-card-title> {{ VideoHandling.formatVideoName(video.name) }} </v-card-title>
        <v-tooltip top>
            <span>{{ video.name }}</span>
        </v-tooltip>
        <v-img @click="openVideoDialog()" style="width: 100%; height: auto"
            :src="`${config.public.baseUrl}/files/${userId}/${video.id}.avif`" />
        <v-tooltip text="Copies url to your clipboard">
            <template v-slot:activator="{ props }">
                <v-btn v-bind="props" @click="copyVideoURL(video.id)" color="success" tile style="width: 100%">
                    Share
                </v-btn>
            </template>
        </v-tooltip>
        <v-divider></v-divider>
        <v-btn @click="openDeleteVideoDialog()" color="error" tile style="width: 100%">
            Delete
        </v-btn>
        <v-dialog style="max-width: 50%;" v-model="dialog">
            <v-card>
                <v-card-title> {{ video.name }} </v-card-title>
                <VideoPlayer :aspect_ratio="'16/9'"
                    :video_url="`${config.public.baseUrl}/files/${userId}/${video.id}.mp4`"
                    :thumb_url="`${config.public.baseUrl}/files/${userId}/${video.id}.avif`" />
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
                    <v-btn color="success" :disabled="timer > 0 || !deleteButtonActive" @click="deleteVideo(video.id)"
                        style="width: 50%">
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
    video: Video,
    userId: string,
}>()
const emit = defineEmits<{
    (e: 'deleteVideo', id: string): void
}>()
const snackbarInformUser = defineModel<boolean>("snackbarInformUser", { required: true });
const snackbarInformUserText = defineModel<string>("snackbarInformUserText", { required: true });
const config = useRuntimeConfig();
const deleteButtonActive = ref(true);
const dialog = ref(false);
const deleteDialog = ref(false);
const timer = ref(0);

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
    deleteButtonActive.value = false;
    const resp = await VideoHandling.deleteVideo(config.public.apiUrl, id)
    deleteButtonActive.value = true;
    if (resp.success) {
        deleteDialog.value = false;
        timer.value = 5;
        emit("deleteVideo", id);
    } else {
        snackbarInformUserText.value = "Video could not be deleted..."
        snackbarInformUser.value = true;
    }
}

function copyVideoURL(id: string) {
    navigator.clipboard.writeText(`${config.public.baseUrl}/video/${id}`);
}

</script>