package results

const (
	NotificationDisabled = 0
	NotificationAPNs = 1
	NotificationFCM = 2
)

type NotificationResult struct {
	Success bool `json:"success"`
} 
