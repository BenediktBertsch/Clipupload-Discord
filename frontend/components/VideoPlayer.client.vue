<template>
    <media-player :style="{'aspect-ratio': aspect_ratio}" class="player" :src="$src" crossOrigin playsInline ref="$player">
        <media-provider>
            <media-poster class="vds-poster" :src="thumb_url" />
        </media-provider>
        <media-audio-layout />
        <media-video-layout />
    </media-player>
</template>

<script setup lang="ts">
import 'vidstack/player/styles/default/theme.css';
import 'vidstack/player/styles/default/layouts/audio.css';
import 'vidstack/player/styles/default/layouts/video.css';
import 'vidstack/player';
import 'vidstack/player/layouts';
import 'vidstack/player/ui';
import type { MediaPlayerElement } from 'vidstack/elements';
import { MediaPlayer } from 'vidstack';

const { video_url, thumb_url, aspect_ratio } = defineProps<{
    video_url: string,
    thumb_url: string,
    aspect_ratio: string,
}>()

const $player = ref<MediaPlayerElement>(),
    $src = ref('');
onMounted(() => {
    $src.value = video_url;
})

</script>