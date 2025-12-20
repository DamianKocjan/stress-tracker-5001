import { DangerZone } from "@/components/auth/danger-zone";
import { EmailSettings } from "@/components/auth/email-settings";
import { ProfileSettings } from "@/components/auth/profile-settings";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs";
import { createFileRoute } from "@tanstack/react-router";

export const Route = createFileRoute("/_authenticated/settings")({
  component: RouteComponent,
});

function RouteComponent() {
  return (
    <div className="min-h-screen p-6 md:p-10">
      <div className="mx-auto max-w-2xl space-y-8">
        <div className="space-y-2">
          <h1 className="text-3xl font-bold">Settings</h1>
          <p className="text-muted-foreground">
            Manage your account and preferences
          </p>
        </div>

        <Tabs defaultValue="profile" className="w-full">
          <TabsList className="grid w-full grid-cols-3">
            <TabsTrigger value="profile">Profile</TabsTrigger>
            <TabsTrigger value="email">Email</TabsTrigger>
            <TabsTrigger value="danger">Danger</TabsTrigger>
          </TabsList>

          <TabsContent value="profile" className="space-y-6 mt-6">
            <ProfileSettings />
          </TabsContent>

          <TabsContent value="email" className="space-y-6 mt-6">
            <EmailSettings />
          </TabsContent>

          <TabsContent value="danger" className="space-y-6 mt-6">
            <DangerZone />
          </TabsContent>
        </Tabs>
      </div>
    </div>
  );
}
