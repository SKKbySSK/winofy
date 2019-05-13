package internal

import (
	"context"
	"firebase.google.com/go"
	"firebase.google.com/go/messaging"
	"google.golang.org/api/option"
)

type FcmNotification struct {
	ctx context.Context
	firebaseApp *firebase.App
	msg *messaging.Client
}

var fcmCredentialsFilePath string

func SetFcmCredentialsFile(path string) {
	fcmCredentialsFilePath = path
}

func CreateFcmNotification() (*FcmNotification, error) {
	ctx := context.Background()
	opt := option.WithCredentialsFile(fcmCredentialsFilePath)

	app, err := firebase.NewApp(ctx, nil, opt)

	res := new(FcmNotification)
	res.ctx = ctx
	res.firebaseApp = app

	if err != nil {
		return nil, err
	}

	msg, err := app.Messaging(ctx)

	if err != nil {
		return nil, err
	}

	res.msg = msg

	return res, nil
}

func (fcm *FcmNotification) SendNotification(title string, body string, fcmToken string) error  {

	msg := &messaging.Message{

		//iOS
		APNS: &messaging.APNSConfig{
			Payload: &messaging.APNSPayload{
				Aps: &messaging.Aps{
					Alert: &messaging.ApsAlert{
						Title: title,
						Body: body,
					},
				},
			},
		},

		Token: fcmToken,
	}

	_, err := fcm.msg.Send(fcm.ctx, msg)

	return err
}
