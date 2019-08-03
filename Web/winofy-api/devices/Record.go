package devices

import "time"

const (
	AxesYZ = 0
	AxesXZ = 1
	AxesXY = 2
)

const (
	WindowOpen = 0
	WindowClose = 1
	WindowNone = 2
)

type Record struct {
	Axes int `json:"axes"`
	Date time.Time `json:"date"`
	SI float32 `json:"si"`
	Window int `json:"window"`
}