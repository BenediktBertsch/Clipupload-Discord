import type { VideosRequest } from "~/utils/schemas"

export abstract class VideoHandling {
    public static async fetchVideos(apiEndpoint: string, count: number = 24, offset: number = 0) {
        const data = await $fetch<VideosRequest>(`${apiEndpoint}/videos?count=${count}&offset=${offset}`, {
            method: "GET",
            headers: {
                Authorization: "Bearer " + localStorage.getItem("token"),
            }
        })
        if (data) {
            return { max: data.max, success: data.success, videoCount: data.videoCount, videos: data.videos, userId: data.userId } as VideosRequest
        }
        return { success: false } as VideosRequest
    }

    public static async deleteVideo(apiEndpoint: string, id: string) {
        const data = await $fetch<VideosRequest>(`${apiEndpoint}/video/${id}`, {
            method: "DELETE",
            headers: {
                Authorization: "Bearer " + localStorage.getItem("token"),
            }
        })
        if (data) {
            return { success: true }
        }
        return { success: false }
    }

    public static formatVideoName(videoname: string) {
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
}