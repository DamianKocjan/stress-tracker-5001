import type { BoardDto } from "@/dto/board.dto";
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

type BoardCardProps = BoardDto;

export function BoardCard({
  id,
  name,
  description,
  ownerId,
  owner,
  updatedAt,
}: BoardCardProps) {
  const { user } = useAuth();

  return (
    <Card className="hover:bg-muted/50 transition-colors">
      <CardHeader>
        <CardTitle>{name}</CardTitle>
        <CardDescription>{description}</CardDescription>
      </CardHeader>
      <CardFooter className="flex flex-col items-start gap-2">
        <div className="flex w-full justify-between text-xs text-muted-foreground">
          {ownerId !== user?.id && <span>By {owner.username}</span>}
          <span>{new Date(updatedAt).toLocaleDateString()}</span>
        </div>
        <Button variant="ghost" className="w-full justify-start p-0" asChild>
          <Link to="/board/$boardId" params={{ boardId: id.toString() }}>
            View Board &rarr;
          </Link>
        </Button>
      </CardFooter>
    </Card>
  );
}
