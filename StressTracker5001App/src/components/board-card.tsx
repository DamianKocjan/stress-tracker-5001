import { useAuth } from "@/providers/auth";
import { Link } from "@tanstack/react-router";
import { Button } from "./ui/button";
import {
  Card,
  CardDescription,
  CardFooter,
  CardHeader,
  CardTitle,
} from "./ui/card";

interface BoardCardProps {
  Id: number;
  Name: string;
  Description: string;
  OwnerId: number;
  Owner: {
    Id: number;
    Email: string;
    Username: string;
    CreatedAt: string;
    UpdatedAt: string;
  };
  CreatedAt: string;
  UpdatedAt: string;
}

export function BoardCard({
  Id,
  Name,
  Description,
  OwnerId,
  Owner,
  UpdatedAt,
}: BoardCardProps) {
  const { user } = useAuth();

  return (
    <Card className="hover:bg-muted/50 transition-colors cursor-pointer">
      <CardHeader>
        <CardTitle>{Name}</CardTitle>
        <CardDescription>{Description}</CardDescription>
      </CardHeader>
      <CardFooter className="flex flex-col items-start gap-2">
        <div className="flex w-full justify-between text-xs text-muted-foreground">
          {OwnerId !== user?.id && <span>By {Owner.Username}</span>}
          <span>{new Date(UpdatedAt).toLocaleDateString()}</span>
        </div>
        <Button variant="ghost" className="w-full justify-start p-0" asChild>
          <Link to={`/_authenticated/dashboard/${Id}`}>View Board &rarr;</Link>
        </Button>
      </CardFooter>
    </Card>
  );
}
